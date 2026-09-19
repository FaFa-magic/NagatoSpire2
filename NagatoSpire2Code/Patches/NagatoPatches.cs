using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using NagatoSpire2.NagatoSpire2Code.Cards;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

/// <summary>
/// Keeps the custom portrait border and title banner un-tinted without changing the rarity
/// material used by the type plaque. The base game intentionally assigns CardModel.BannerMaterial
/// to all three nodes, so these node-local corrections run after every relevant visual refresh.
/// </summary>
[HarmonyAfter("com.ritsukage.sts2-RitsuLib.framework-content-assets")]
internal sealed class NagatoCardChromePatch : IPatchMethod
{
	private const string PlaquePathPrefix =
		"res://NagatoSpire2/images/card_frames/nagato_type_plaque_";

	private static readonly ConditionalWeakTable<NinePatchRect, OriginalPlaqueTexture> OriginalPlaques = new();

	private static Texture2D? _commonPlaque;
	private static Texture2D? _uncommonPlaque;
	private static Texture2D? _rarePlaque;

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
		NinePatchRect? typePlaque = __instance.GetNodeOrNull<NinePatchRect>("%TypePlaque");

		if (__instance.Model is not NagatoCardModel model || model.Rarity == CardRarity.Ancient)
		{
			RestorePlaqueIfNeeded(typePlaque);
			return;
		}

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

		if (typePlaque == null)
			return;

		OriginalPlaques.GetValue(typePlaque, static plaque => new OriginalPlaqueTexture(plaque.Texture));
		Texture2D? texture = LoadPlaque(model.Rarity);
		if (texture != null)
			typePlaque.Texture = texture;

		typePlaque.UseParentMaterial = false;
		typePlaque.Material = NagatoCardModel.UnfilteredChromeMaterial;
	}

	private static Texture2D? LoadPlaque(CardRarity rarity)
	{
		return rarity switch
		{
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
}
