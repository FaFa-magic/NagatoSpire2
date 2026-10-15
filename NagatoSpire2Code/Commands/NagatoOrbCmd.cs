using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Commands;

public static class NagatoOrbCmd
{
	private static readonly Func<PlayerChoiceContext, Player, OrbModel, bool, Task> EvokeOrb =
		AccessTools.MethodDelegate<Func<PlayerChoiceContext, Player, OrbModel, bool, Task>>(
			AccessTools.DeclaredMethod(typeof(OrbCmd), "Evoke",
				[typeof(PlayerChoiceContext), typeof(Player), typeof(OrbModel), typeof(bool)]));

	public static async Task Evoke(PlayerChoiceContext choiceContext, Player player, IEnumerable<OrbModel> targets)
	{
		if (player.PlayerCombatState is not { } state)
			return;
		foreach (var orb in targets.Distinct().ToArray())
		{
			if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead || player.PlayerCombatState != state)
				break;
			if (state.OrbQueue.Orbs.Contains(orb))
				await EvokeOrb(choiceContext, player, orb, true);
		}
	}
}
