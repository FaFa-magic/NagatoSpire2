using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using NagatoSpire2.NagatoSpire2Code.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Cards;

[RegisterCard(typeof(NagatoCardPool), Inherit = true)]
public abstract class NagatoCardModel : ModCardTemplate
{
	private const string PortraitBorderPathPrefix =
		"res://NagatoSpire2/images/card_frames/nagato_portrait_border_";
	internal static readonly Material UnfilteredChromeMaterial = new CanvasItemMaterial
	{
		LightMode = CanvasItemMaterial.LightModeEnum.Unshaded
	};

	protected NagatoCardModel(
		int energyCost,
		CardType type,
		CardRarity rarity,
		TargetType targetType,
		bool shouldShowInCardLibrary = true)
		: base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
	{
	}

	public override CardAssetProfile AssetProfile => new(
		PortraitPath: $"res://NagatoSpire2/images/cards/{GetType().Name}.png",
		BannerTexturePath: "res://NagatoSpire2/images/card_frames/nagato_Banner.png",
		BannerMaterial: UnfilteredChromeMaterial,
		PortraitBorderPath: ResolvePortraitBorderPath(),
		PortraitBorderMaterial: UnfilteredChromeMaterial,
		AncientBannerPath: "res://NagatoSpire2/images/card_frames/nagato_ancient_Banner.png",
		AncientBorderPath: "res://NagatoSpire2/images/card_frames/nagato_ancient.png",
		AncientBorderMaterialPath: "res://NagatoSpire2/materials/cards/nagato_ancient_border_opaque.tres",
		FramePath: Type switch
		{
			CardType.Attack => "res://NagatoSpire2/images/card_frames/nagato_attack.png",
			CardType.Skill => "res://NagatoSpire2/images/card_frames/nagato_skill.png",
			CardType.Power => "res://NagatoSpire2/images/card_frames/nagato_power.png",
			_ => ""
		}
	);

	private string? ResolvePortraitBorderPath()
	{
		if (Rarity == CardRarity.Ancient)
			return null;

		string? typeSuffix = Type switch
		{
			CardType.Attack => "attack",
			CardType.Skill => "skill",
			CardType.Power => "power",
			_ => null
		};

		if (typeSuffix == null)
			return null;

		string rarityPrefix = Rarity switch
		{
			CardRarity.Basic or CardRarity.Common => "common_",
			CardRarity.Rare => "rare_",
			_ => ""
		};

		return $"{PortraitBorderPathPrefix}{rarityPrefix}{typeSuffix}.png";
	}
}
