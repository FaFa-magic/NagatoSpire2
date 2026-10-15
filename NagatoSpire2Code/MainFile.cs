using System.Reflection;
using NagatoSpire2.NagatoSpire2Code.Acts;
using NagatoSpire2.NagatoSpire2Code.Audio;
using NagatoSpire2.NagatoSpire2Code.Configs;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Networking;
using MegaCrit.Sts2.Core.Modding;
using NagatoSpire2.NagatoSpire2Code.Patches;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace NagatoSpire2.NagatoSpire2Code;

[ModInitializer(nameof(Initialize))]
public static class MainFile
{
	public const string ModId = "NagatoSpire2";

	public static Logger Logger { get; private set; } = null!;

	public static void Initialize()
	{
		var assembly = Assembly.GetExecutingAssembly();

		Logger = RitsuLibFramework.CreateLogger(ModId);
		ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
		RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
		NagatoConfigPage.Register();
		NagatoAudio.Register();
		NagatoCombatStartVoice.Register();
		NagatoFleetVoteSync.Register();
		InkStainedSteelSakuraRun.Register();
		ModHelper.SubscribeForCombatStateHooks("nagato_card_bound_orbs", state =>
			state.Players.Where(player => player.IsActiveForHooks).SelectMany(NagatoCardOrbs.GetAll));

		var actPatcher = RitsuLibFramework.CreatePatcher(ModId, "nagato_fourth_act_patches");
		actPatcher.RegisterPatch<InkStainedSteelSakuraRunPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraMapPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraFloorCountPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraMapRestorePatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraActLoadPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraActSerializePatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraBossSavePatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraMultiplayerScalingPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraTreasureRoomPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraRestSiteActIndexPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraRestSiteReadyPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraRestSiteLayoutPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraBossRewardsPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraMapMarkerPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraMapTexturePatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraBossIconPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraTopBarBossPatch>();
		actPatcher.RegisterPatch<InkStainedSteelSakuraTopBarRoomPatch>();
		if (!actPatcher.PatchAll())
			throw new InvalidOperationException("Critical Nagato fourth-act patches failed.");

		var audioPatcher = RitsuLibFramework.CreatePatcher(ModId, "nagato_audio_patches");
		audioPatcher.RegisterPatch<NagatoCharacterSelectVoicePatch>();
		audioPatcher.RegisterPatch<NagatoAttackAudioContextPatch>();
		audioPatcher.RegisterPatch<NagatoAttackAudioPlaybackPatch>();
		if (!audioPatcher.PatchAll())
			Logger.ErrorNoTrace("Nagato audio lifecycle patches failed to apply.");

		var cardVisualPatcher = RitsuLibFramework.CreatePatcher(ModId, "card-visuals");
		cardVisualPatcher.RegisterPatch<NagatoCardChromePatch>();
		cardVisualPatcher.RegisterPatch<NagatoChoiceScreenPatch>();
		cardVisualPatcher.RegisterPatch<NagatoCardOrbVisualPatch>();
		cardVisualPatcher.RegisterPatch<NagatoWhiteLightVisualBootstrapPatch>();
		if (!cardVisualPatcher.PatchAll())
			Logger.ErrorNoTrace("Nagato card visual patches failed to apply.");

		var transitionPatcher = RitsuLibFramework.CreatePatcher(ModId, "nagato_transition_patches");
		transitionPatcher.RegisterPatch<NagatoTransitionPatch>();
		if (!transitionPatcher.PatchAll())
			Logger.ErrorNoTrace("Nagato character transition patch failed to apply.");

		var spinePatcher = RitsuLibFramework.CreatePatcher(ModId, "nagato_spine_patches");
		spinePatcher.RegisterPatch<NagatoSkinEnumerationPatch>();
		spinePatcher.RegisterPatch<NagatoSkinCardLibrarySelectionPatch>();
		spinePatcher.RegisterPatch<NagatoSharedProgressionLookupPatch>();
		spinePatcher.RegisterPatch<NagatoSkinAncientDialogueLookupPatch>();
		spinePatcher.RegisterPatch<NagatoSharedGameOverProgressionPatch>();
		spinePatcher.RegisterPatch<NagatoSkinSelectPatch>();
		spinePatcher.RegisterPatch<NagatoSkinSelectEmbarkPatch>();
		spinePatcher.RegisterPatch<NagatoSkinSelectUnreadyPatch>();
		spinePatcher.RegisterPatch<NagatoCombatSpineIdleBootstrapPatch>();

		if (!spinePatcher.PatchAll())
			throw new InvalidOperationException("Critical Nagato Spine patches failed.");

		var orbPatcher = RitsuLibFramework.CreatePatcher(ModId, "nagato_orb_patches");
		orbPatcher.RegisterPatch<NagatoOrbHistoryPatches.RecordPatch>();
		orbPatcher.RegisterPatch<NagatoOrbHistoryPatches.ClearPatch>();
		if (!orbPatcher.PatchAll())
			throw new InvalidOperationException("Critical Nagato orb patches failed.");

		var previewPatcher = RitsuLibFramework.CreatePatcher(ModId, "draw-pile-previews");
		previewPatcher.RegisterPatch<NagatoDrawPilePreviewPatches.InitializePatch>();
		previewPatcher.RegisterPatch<NagatoDrawPilePreviewPatches.AddPatch>();
		previewPatcher.RegisterPatch<NagatoDrawPilePreviewPatches.RemovePatch>();
		previewPatcher.RegisterPatch<NagatoDrawPilePreviewPatches.ShufflePatch>();
		previewPatcher.RegisterPatch<NagatoDrawPilePreviewPatches.RandomizePatch>();
		previewPatcher.RegisterPatch<NagatoDrawPilePreviewPatches.MoveToTopPatch>();
		previewPatcher.RegisterPatch<NagatoDrawPilePreviewPatches.TransformPatch>();
		if (!previewPatcher.PatchAll())
			Logger.ErrorNoTrace("Nagato draw-pile preview patches failed to apply.");

		var platingPatcher = RitsuLibFramework.CreatePatcher(ModId, "nagato_plating_patches");
		platingPatcher.RegisterPatch<NagatoPlatingTriggerPatch>();
		platingPatcher.RegisterPatch<NagatoPlatingTurnStartPatch>();
		if (!platingPatcher.PatchAll())
			throw new InvalidOperationException("Critical Nagato Plating patches failed.");

		var drawPatcher = RitsuLibFramework.CreatePatcher(ModId, "nagato_draw_patches");
		drawPatcher.RegisterPatch<NagatoCardDrawnPatch>();
		drawPatcher.RegisterPatch<NagatoSealedAutoplayPatch>();
		if (!drawPatcher.PatchAll())
			throw new InvalidOperationException("Critical Nagato card draw patches failed.");

		var damagePatcher = RitsuLibFramework.CreatePatcher(ModId, "nagato_orb_damage_patches");
		damagePatcher.RegisterPatch<NagatoOrbDamagePatch>();
		if (!damagePatcher.PatchAll())
			throw new InvalidOperationException("Critical Nagato orb damage source patch failed.");
	}
}
