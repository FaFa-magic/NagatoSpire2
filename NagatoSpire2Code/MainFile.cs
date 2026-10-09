using System.Reflection;
using NagatoSpire2.NagatoSpire2Code.Audio;
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
		NagatoAudio.Register();
		NagatoCombatStartVoice.Register();

		var audioPatcher = RitsuLibFramework.CreatePatcher(ModId, "nagato_audio_patches");
		audioPatcher.RegisterPatch<NagatoCharacterSelectVoicePatch>();
		audioPatcher.RegisterPatch<NagatoAttackAudioContextPatch>();
		audioPatcher.RegisterPatch<NagatoAttackAudioPlaybackPatch>();
		if (!audioPatcher.PatchAll())
			Logger.ErrorNoTrace("Nagato audio lifecycle patches failed to apply.");

		var cardVisualPatcher = RitsuLibFramework.CreatePatcher(ModId, "card-visuals");
		cardVisualPatcher.RegisterPatch<NagatoCardChromePatch>();
		cardVisualPatcher.RegisterPatch<NagatoChoiceScreenPatch>();
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
		orbPatcher.RegisterPatch<NagatoOrbEvokePatch>();
		orbPatcher.RegisterPatch<NagatoOrbChannelPatch>();
		orbPatcher.RegisterPatch<NagatoOrbTargetingPatches.MousePatch>();
		orbPatcher.RegisterPatch<NagatoOrbTargetingPatches.MousePlayZonePatch>();
		orbPatcher.RegisterPatch<NagatoOrbTargetingPatches.ControllerPatch>();
		orbPatcher.RegisterPatch<NagatoOrbTargetingPatches.LayoutPatch>();
		if (!orbPatcher.PatchAll())
			throw new InvalidOperationException("Critical Nagato orb patches failed.");

		var platingPatcher = RitsuLibFramework.CreatePatcher(ModId, "nagato_plating_patches");
		platingPatcher.RegisterPatch<NagatoPlatingTriggerPatch>();
		if (!platingPatcher.PatchAll())
			throw new InvalidOperationException("Critical Nagato Plating patches failed.");
	}
}
