using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using NagatoSpire2.NagatoSpire2Code.Cards;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Utils;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

/// <summary>
/// Keeps the custom title banner un-tinted without changing the rarity material used by the
/// portrait border and type plaque. The base game intentionally assigns CardModel.BannerMaterial
/// to all three nodes, so this node-local correction runs after the normal card reload pipeline.
/// </summary>
[HarmonyAfter("com.ritsukage.sts2-RitsuLib.framework-content-assets")]
internal sealed class NagatoCardChromePatch : IPatchMethod
{
	private const string PlaquePathPrefix =
		"res://NagatoSpire2/images/card_frames/nagato_type_plaque_";

	private static readonly ConditionalWeakTable<NinePatchRect, OriginalPlaqueTexture> OriginalPlaques = new();
	private static readonly Material? TitleBannerMaterial = MaterialUtils.CreateUnmodulatedHsvShaderMaterial();

	private static Texture2D? _attackPlaque;
	private static Texture2D? _skillPlaque;
	private static Texture2D? _powerPlaque;

	public static string PatchId => "nagato_card_chrome_reload";
	public static string Description => "Decouple Nagato title banner and type plaque visuals";

	public static ModPatchTarget[] GetTargets() =>
	[
		PatchTarget.Method<NCard>("Reload")
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

		TextureRect? titleBanner = __instance.GetNodeOrNull<TextureRect>("%TitleBanner");
		if (titleBanner != null)
			titleBanner.Material = TitleBannerMaterial;

		if (typePlaque == null)
			return;

		OriginalPlaques.GetValue(typePlaque, static plaque => new OriginalPlaqueTexture(plaque.Texture));
		Texture2D? texture = LoadPlaque(model.Type);
		if (texture != null)
			typePlaque.Texture = texture;
	}

	private static Texture2D? LoadPlaque(CardType type)
	{
		return type switch
		{
			CardType.Attack => LoadTexture(ref _attackPlaque, $"{PlaquePathPrefix}attack.png"),
			CardType.Skill => LoadTexture(ref _skillPlaque, $"{PlaquePathPrefix}skill.png"),
			CardType.Power => LoadTexture(ref _powerPlaque, $"{PlaquePathPrefix}power.png"),
			_ => null
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
