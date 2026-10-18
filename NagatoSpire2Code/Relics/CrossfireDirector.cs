using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Relics;

public sealed class CrossfireDirector : NagatoRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Uncommon;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(2m, ValueProp.Unpowered)];

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
		ValueProp props, Creature target, CardModel? cardSource)
	{
		if (cardSource?.Owner != Owner || target.Side != CombatSide.Enemy ||
			result.TotalDamage <= 0 || CombatManager.Instance.IsEnding || Owner.Creature.CombatState is not { } combatState)
			return;
		var targets = combatState.HittableEnemies.Where(enemy => enemy != target).ToArray();
		if (targets.Length == 0)
			return;
		Flash();
		await CreatureCmd.Damage(choiceContext, targets, DynamicVars.Damage, Owner.Creature);
	}
}
