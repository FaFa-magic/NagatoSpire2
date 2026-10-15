using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using NagatoSpire2.NagatoSpire2Code.Cards;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

/// <summary>
/// Keeps Nagato's custom chrome un-tinted and selects type-plaque artwork by rarity,
/// including one shared ivory plaque for all Ancient card types. The base game assigns
/// CardModel.BannerMaterial to these nodes, so corrections run after visual refreshes.
/// Aligns the outer frame, portrait rails and overhanging banner as one ordinary torii frame.
/// Leaves highlight sizing and animations entirely to the base game.
/// </summary>
[HarmonyAfter("com.ritsukage.sts2-RitsuLib.framework-content-assets")]
internal sealed class NagatoCardChromePatch : IPatchMethod
{
	private const string PlaquePathPrefix =
		"res://NagatoSpire2/images/card_frames/nagato_type_plaque_";

	private static readonly ConditionalWeakTable<NinePatchRect, OriginalPlaqueTexture> OriginalPlaques = new();
	private static readonly ConditionalWeakTable<TextureRect, OriginalTextureLayout> OriginalLayouts = new();
	private const float ToriiFrameWidthScale = 1.06f;
	private const float ToriiPortraitBorderWidthScale = 1.14f;
	private const float ToriiPortraitScale = 1.10f;
	private const float ToriiBannerScale = 1.12f;

	private static Texture2D? _commonPlaque;
	private static Texture2D? _uncommonPlaque;
	private static Texture2D? _rarePlaque;
	private static Texture2D? _ancientPlaque;

	public static string PatchId => "nagato_card_chrome_refresh";
	public static string Description => "Decouple Nagato chrome and select type plaques by rarity";

	public static ModPatchTarget[] GetTargets() =>
	[
		PatchTarget.Method<NCard>("Reload"),
		PatchTarget.Method<NCard>("UpdatePortrait"),
		PatchTarget.Method<NCard>("UpdateVisuals")
	];

	[HarmonyPostfix]
	private static void Postfix(NCard __instance)
	{
		UpdateToriiLayout(__instance);

		NinePatchRect? typePlaque = __instance.GetNodeOrNull<NinePatchRect>("%TypePlaque");

		if (__instance.Model is not NagatoCardModel model)
		{
			RestorePlaqueIfNeeded(typePlaque);
			return;
		}

		// Ancient cards have separate border/banner nodes; only their type plaque
		// is customized here. Leave the established Ancient frame binding intact.
		if (model.Rarity != CardRarity.Ancient)
		{
			TextureRect? portraitBorder = __instance.GetNodeOrNull<TextureRect>("%PortraitBorder");
			TextureRect? titleBanner = __instance.GetNodeOrNull<TextureRect>("%TitleBanner");

			if (portraitBorder != null)
			{
				portraitBorder.UseParentMaterial = false;
				portraitBorder.Material = NagatoCardModel.UnfilteredChromeMaterial;
			}

			if (titleBanner != null)
			{
				titleBanner.UseParentMaterial = false;
				titleBanner.Material = NagatoCardModel.UnfilteredChromeMaterial;
			}
		}

		if (typePlaque == null)
			return;

		OriginalPlaques.GetValue(typePlaque, static plaque => new OriginalPlaqueTexture(plaque.Texture));
		Texture2D? texture = LoadPlaque(model.Rarity);
		if (texture != null)
			typePlaque.Texture = texture;

		typePlaque.UseParentMaterial = false;
		typePlaque.Material = NagatoCardModel.UnfilteredChromeMaterial;
	}

	private static void UpdateToriiLayout(NCard card)
	{
		bool useToriiLayout = card.Model is NagatoCardModel model && model.Rarity != CardRarity.Ancient &&
			model.Type is CardType.Attack or CardType.Skill or CardType.Power;
		ApplyNodeLayout(card, "%Frame", useToriiLayout, new Vector2(ToriiFrameWidthScale, 1f));
		ApplyNodeLayout(card, "%PortraitBorder", useToriiLayout, new Vector2(ToriiPortraitBorderWidthScale, 1f));
		// Fill the wider opening with an equal-axis zoom, preserving the illustration's proportions.
		ApplyNodeLayout(card, "%Portrait", useToriiLayout, Vector2.One * ToriiPortraitScale);
		ApplyNodeLayout(card, "%TitleBanner", useToriiLayout, Vector2.One * ToriiBannerScale);
	}

	private static void ApplyNodeLayout(NCard card, string path, bool useToriiLayout, Vector2 factor)
	{
		TextureRect? node = card.GetNodeOrNull<TextureRect>(path);
		if (node == null)
			return;

		if (!useToriiLayout)
		{
			if (OriginalLayouts.TryGetValue(node, out OriginalTextureLayout? original))
			{
				original.Apply(node, Vector2.One);
				OriginalLayouts.Remove(node);
			}
			return;
		}

		// Existing TextureRects only: no additional render pass, texture, shader or overlay.
		OriginalLayouts.GetValue(node, static texture => new OriginalTextureLayout(texture)).Apply(node, factor);
	}

	private static Texture2D? LoadPlaque(CardRarity rarity)
	{
		return rarity switch
		{
			CardRarity.Ancient =>
				LoadTexture(ref _ancientPlaque, $"{PlaquePathPrefix}ancient.png"),
			CardRarity.Basic or CardRarity.Common =>
				LoadTexture(ref _commonPlaque, $"{PlaquePathPrefix}common.png"),
			CardRarity.Rare =>
				LoadTexture(ref _rarePlaque, $"{PlaquePathPrefix}rare.png"),
			_ => LoadTexture(ref _uncommonPlaque, $"{PlaquePathPrefix}uncommon.png")
		};
	}

	private static Texture2D? LoadTexture(ref Texture2D? texture, string path)
	{
		if (texture == null || !GodotObject.IsInstanceValid(texture))
		{
			texture = ResourceLoader.Load<Texture2D>(
				path,
				null,
				ResourceLoader.CacheMode.Reuse);
		}

		return texture;
	}

	private static void RestorePlaqueIfNeeded(NinePatchRect? typePlaque)
	{
		if (typePlaque == null ||
			!OriginalPlaques.TryGetValue(typePlaque, out OriginalPlaqueTexture? original) ||
			typePlaque.Texture?.ResourcePath.StartsWith(PlaquePathPrefix, StringComparison.Ordinal) != true)
		{
			return;
		}

		typePlaque.Texture = original.Value;
	}

	private sealed class OriginalPlaqueTexture(Texture2D? value)
	{
		public Texture2D? Value { get; } = value;
	}

	private sealed class OriginalTextureLayout(TextureRect node)
	{
		private readonly float _left = node.OffsetLeft;
		private readonly float _top = node.OffsetTop;
		private readonly float _right = node.OffsetRight;
		private readonly float _bottom = node.OffsetBottom;
		private readonly Vector2 _scale = node.Scale;
		private readonly Vector2 _centerFromPivot = node.Size * 0.5f - node.PivotOffset;

		public void Apply(TextureRect node, Vector2 factor)
		{
			// Derive from the captured layout every refresh; never accumulate scaling on pooled cards.
			Vector2 scale = _scale * factor;
			Vector2 shift = (_centerFromPivot * (_scale - scale)).Rotated(node.Rotation);
			node.Scale = scale;
			node.OffsetLeft = _left + shift.X;
			node.OffsetRight = _right + shift.X;
			node.OffsetTop = _top + shift.Y;
			node.OffsetBottom = _bottom + shift.Y;
		}
	}

}
