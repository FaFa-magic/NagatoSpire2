using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class SympatheticDetonationPower : NagatoPowerModel
{
	private static readonly AsyncLocal<bool> Resolving = new();

	public override PowerType Type => PowerType.Debuff;
	public override PowerStackType StackType => PowerStackType.Single;
	protected override bool UsesPackedIcon => true;

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer,
		DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		if (target != Owner || result.TotalDamage <= 0 || Resolving.Value || CombatManager.Instance.IsOverOrEnding ||
			Owner.CombatState is not { } combat)
			return;
		var targets = combat.HittableEnemies.Where(creature => creature != Owner).ToArray();
		if (targets.Length == 0)
			return;
		Resolving.Value = true;
		try
		{
			Flash();
			await CreatureCmd.Damage(choiceContext, targets, result.TotalDamage * Amount / 100m, ValueProp.Unpowered, dealer, null, null);
		}
		finally
		{
			Resolving.Value = false;
		}
	}

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
		IEnumerable<Creature> participants)
	{
		if (side == CombatSide.Player)
			await PowerCmd.Remove(this);
	}
}
