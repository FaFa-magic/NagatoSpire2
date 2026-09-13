using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;

namespace NagatoSpire2.NagatoSpire2Code.Characters;

[RegisterCharacter]
public sealed class NagatoCharacter : ModCharacterTemplate<NagatoCardPool, NagatoRelicPool, NagatoPotionPool>
{
	public const string CharacterId = "Nagato";
	public const string CharacterColor = "Nagato_sakura";

	public override CharacterGender Gender => CharacterGender.Feminine;
	public override int StartingHp => 70;
	public override int StartingGold => 99;

	public override Color NameColor => new("#9A72A1");
	public override Color EnergyLabelOutlineColor => new("1E283CFF");
	public override Color MapDrawingColor => new("#9A72A1");
	public override Color DialogueColor => new("#9A72A1");
	public override Color RemoteTargetingLineColor => new("#AAAAAA");
	public override Color RemoteTargetingLineOutline => Colors.Black;

	public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
		CharacterAssetProfiles.Ironclad(),
		new(
			Scenes: new(
				VisualsPath: "res://NagatoSpire2/scenes/characters/Nagato.tscn",
				EnergyCounterPath: "res://NagatoSpire2/scenes/vfx/nagato_energy_counter.tscn",
				MerchantAnimPath: "res://NagatoSpire2/scenes/characters/Nagato_merchant.tscn",
				RestSiteAnimPath: "res://NagatoSpire2/scenes/characters/Nagato_rest_site.tscn"
			),
			Ui: new(
				IconTexturePath: "res://NagatoSpire2/images/characters/character_icon_nagato.png",
				IconOutlineTexturePath: "res://NagatoSpire2/images/characters/character_icon_nagato_outline.png",
				IconPath: "res://NagatoSpire2/scenes/characters/Nagato_icon.tscn",
				CharacterSelectBgPath: "res://NagatoSpire2/scenes/characters/char_select_bg_Nagato.tscn",
				CharacterSelectIconPath: "res://NagatoSpire2/images/characters/char_select_nagato.png",
				CharacterSelectLockedIconPath: "res://NagatoSpire2/images/characters/char_select_nagato_locked.png",
				CharacterSelectTransitionPath: "res://NagatoSpire2/materials/nagato_transition_mat.tres",
				MapMarkerPath: "res://NagatoSpire2/images/characters/map_marker_nagato.png"
			),
			Vfx: new(
				TrailPath: "res://NagatoSpire2/scenes/vfx/card_trail_nagato.tscn"
			),
			VanillaRelicVisualOverrides:
			[
				new(CharacterOwnedVanillaRelicModelId.YummyCookie, new(
					"res://NagatoSpire2/images/relics/packed/YummyCookie_Nagato.png",
					"res://NagatoSpire2/images/relics/outline/YummyCookie_Nagato.png",
					"res://NagatoSpire2/images/relics/big/YummyCookie_Nagato.png"
				))
			]
		));

	public override float AttackAnimDelay => 0f;
	public override float CastAnimDelay => 0f;
	public override bool RequiresEpochAndTimeline => false;

	protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
		RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

	public override List<string> GetArchitectAttackVfx() =>
	[
		"vfx/vfx_attack_blunt",
		"vfx/vfx_heavy_blunt",
		"vfx/vfx_attack_slash",
		"vfx/vfx_bloody_impact",
		"vfx/vfx_rock_shatter"
	];
}
