using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Combat;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public static class NagatoOrbHistoryPatches
{
	public sealed class RecordPatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_evoke_history";
		public static string Description => "Record resolved orb evokes in the current combat history";
		public static bool IsCritical => true;

		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(Hook), nameof(Hook.AfterOrbEvoked),
				[typeof(PlayerChoiceContext), typeof(ICombatState), typeof(OrbModel), typeof(IEnumerable<Creature>)])
		];

		[HarmonyPrefix]
		public static void Prefix(ICombatState combatState, OrbModel orb) => NagatoOrbHistory.Record(combatState, orb);
	}

	public sealed class ClearPatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_evoke_history_clear";
		public static string Description => "Clear attached orb evoke counts with the official combat history";
		public static bool IsCritical => true;

		public static ModPatchTarget[] GetTargets() => [new(typeof(CombatHistory), nameof(CombatHistory.Clear), [])];

		[HarmonyPostfix]
		public static void Postfix(CombatHistory __instance) => NagatoOrbHistory.Clear(__instance);
	}
}
