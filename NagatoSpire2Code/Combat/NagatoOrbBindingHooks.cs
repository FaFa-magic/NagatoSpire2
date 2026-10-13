using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

[RegisterSingleton]
public sealed class NagatoOrbBindingHooks() : HookedSingletonModel(HookType.Combat)
{
	public override Task BeforeCombatStart()
	{
		if (CurrentCombatState != null)
			foreach (var player in CurrentCombatState.Players)
				NagatoOrbBindings.Initialize(player);
		return Task.CompletedTask;
	}

	public override Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb) =>
		NagatoOrbBindings.Reconcile(choiceContext, player);

	public override Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets) =>
		NagatoOrbBindings.Reconcile(choiceContext, orb.Owner);

	public override Task AfterCombatEnd(CombatRoom room)
	{
		foreach (var player in room.CombatState.Players)
			NagatoOrbBindings.Clear(player);
		return Task.CompletedTask;
	}
}
