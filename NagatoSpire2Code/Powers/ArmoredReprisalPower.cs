using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class ArmoredReprisalPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;

	public override PowerStackType StackType => PowerStackType.Counter;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	public async Task AfterPlatingTriggered(PlayerChoiceContext choiceContext, int platingAmount)
	{
		if (platingAmount <= 0 || Owner.CombatState is not { } combatState ||
			Owner.Player is not { PlayerCombatState: { } state } player)
			return;

		int repeats = Amount;
		for (int i = 0; i < repeats; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.IsDead || player.PlayerCombatState != state || Owner.CombatState != combatState)
				break;

			var enemies = combatState.HittableEnemies;
			if (enemies.Count == 0)
				break;

			var target = player.RunState.Rng.CombatTargets.NextItem(enemies);
			if (target == null)
				break;
			Flash();
			await CreatureCmd.Damage(choiceContext, target, platingAmount, ValueProp.Unpowered, Owner);
		}
	}
}
