using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class GunneryDrills() : NagatoCardModel(2, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
{
	public override int MaxUpgradeLevel => int.MaxValue;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Move), new IntVar("Hits", 3)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combat)
			return;
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars["Hits"].IntValue)
			.FromCard(this, cardPlay).TargetingRandomOpponents(combat)
			.WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(1m);
		DynamicVars["Hits"].UpgradeValueBy(1m);
	}
}
