using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace NagatoSpire2.NagatoSpire2Code.Characters;

[RegisterCharacter]
public class NagatoCharacter : ModCharacterTemplate<NagatoCardPool, NagatoRelicPool, NagatoPotionPool>
{
	public const string CharacterId = "Nagato";
	public const string CharacterColor = "Nagato_sakura";
	public virtual NagatoSkin CurrentSkin => NagatoSkin.Default;
	public NagatoSkinDefinition CurrentSkinDefinition => NagatoSkinManager.GetDefinition(CurrentSkin);

	public override CharacterGender Gender => CharacterGender.Feminine;
	public override int StartingHp => 70;
	public override int StartingGold => 99;
	public override int BaseOrbSlotCount => 8;

	public override Color NameColor => new("#9A72A1");
	public override Color EnergyLabelOutlineColor => new("521326FF");
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
				MerchantAnimPath: CurrentSkinDefinition.MerchantAnimPath,
				RestSiteAnimPath: CurrentSkinDefinition.RestSiteAnimPath
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
			Spine: new(
				CombatSkeletonDataPath: CurrentSkinDefinition.SpineSkeletonDataPath
			),
			Multiplayer: new(
				ArmPointingTexturePath: "res://NagatoSpire2/images/characters/hands/multiplayer_hand_nagato_point.png",
				ArmRockTexturePath: "res://NagatoSpire2/images/characters/hands/multiplayer_hand_nagato_rock.png",
				ArmPaperTexturePath: "res://NagatoSpire2/images/characters/hands/multiplayer_hand_nagato_paper.png",
				ArmScissorsTexturePath: "res://NagatoSpire2/images/characters/hands/multiplayer_hand_nagato_scissors.png"
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

	protected override NCreatureVisuals? TryCreateCreatureVisuals()
	{
		var visuals = RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);
		if (visuals is null || CurrentSkin == NagatoSkin.Default)
			return visuals;

		// The game-over screen creates visuals directly, without NCreature._Ready.
		var spineNode = visuals.GetNodeOrNull<Node2D>("%Visuals");
		if (!GodotObject.IsInstanceValid(spineNode) || spineNode.GetClass() != MegaSprite.spineClassName)
			return visuals;

		var skeletonData = ResourceLoader.Load<Resource>(CurrentSkinDefinition.SpineSkeletonDataPath);
		if (skeletonData is not null)
			new MegaSprite((Variant)(GodotObject)spineNode).SetSkeletonDataRes(new MegaSkeletonDataResource(skeletonData));

		return visuals;
	}

	protected override CreatureAnimator? SetupCustomCreatureAnimator(MegaSprite controller) =>
		ModAnimStateMachines.Standard(
			controller,
			idleName: "normal",
			deadName: "dead",
			hitName: "touch",
			attackName: "attack",
			castName: "attack_left",
			relaxedName: "sleep");

	public override List<string> GetArchitectAttackVfx() =>
	[
		"vfx/vfx_attack_blunt",
		"vfx/vfx_heavy_blunt",
		"vfx/vfx_attack_slash",
		"vfx/vfx_bloody_impact",
		"vfx/vfx_rock_shatter"
	];
}
