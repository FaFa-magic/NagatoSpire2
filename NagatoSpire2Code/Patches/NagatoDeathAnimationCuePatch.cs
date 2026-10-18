using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using NagatoSpire2.NagatoSpire2Code.Characters;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Characters.Visuals;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoDeathAnimationCuePatch : IPatchMethod
{
	public static string PatchId => "nagato_death_animation_cue";
	public static string Description => "Use Nagato's dead animation for the default death cue";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(ModCreatureVisualPlayback), nameof(ModCreatureVisualPlayback.TryPlayCue),
			[typeof(NCreatureVisuals), typeof(CharacterModel), typeof(string),
				typeof(ReadOnlySpan<string>), typeof(bool)])
	];

	[HarmonyPrefix]
	public static void Prefix(
		CharacterModel? character,
		ref string primaryCue,
		ReadOnlySpan<string> alternateCueNames)
	{
		// Spine can log a missing animation without throwing, stopping alias fallback.
		// Preserve explicit cue lists and every other character's animation names.
		if (character is NagatoCharacter && alternateCueNames.IsEmpty &&
		    string.Equals(primaryCue, "die", StringComparison.OrdinalIgnoreCase))
		{
			primaryCue = "dead";
		}
	}
}
