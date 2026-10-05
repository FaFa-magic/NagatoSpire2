using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Patches;

namespace NagatoSpire2.NagatoSpire2Code.Commands;

public static class NagatoOrbCmd
{
	private static readonly Func<PlayerChoiceContext, Player, OrbModel, bool, Task> EvokeOrb =
		AccessTools.MethodDelegate<Func<PlayerChoiceContext, Player, OrbModel, bool, Task>>(
			AccessTools.DeclaredMethod(typeof(OrbCmd), "Evoke",
				[typeof(PlayerChoiceContext), typeof(Player), typeof(OrbModel), typeof(bool)]));

	public static async Task Evoke(PlayerChoiceContext choiceContext, Player player, IEnumerable<OrbModel> targets)
	{
		if (player.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding)
			return;
		OrbModel[] orbs = targets.Distinct().ToArray();
		if (orbs.Length == 0)
			return;

		NagatoOrbResolutionScope.Enter(player);
		try
		{
			foreach (OrbModel orb in orbs)
			{
				if (CombatManager.Instance.IsOverOrEnding)
					break;
				if (!state.OrbQueue.Orbs.Contains(orb))
					continue;

				choiceContext.PushModel(orb);
				try
				{
					await EvokeOrb(choiceContext, player, orb, true);
				}
				finally
				{
					choiceContext.PopModel(orb);
				}
			}
		}
		finally
		{
			await NagatoOrbResolutionScope.Exit(player, choiceContext);
		}
	}
}
