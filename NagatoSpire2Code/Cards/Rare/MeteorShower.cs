using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class MeteorShower() : NagatoCardModel(0, CardType.Attack, CardRarity.Rare, TargetType.RandomEnemy)
{
	protected override bool HasEnergyCostX => true;
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(1m, ValueProp.Move), new IntVar("Hits", 1), new EnergyVar(1)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [EnergyHoverTip];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combatState)
			return;
		int x = ResolveEnergyXValue();
		await DamageCmd.Attack(x + DynamicVars.Damage.BaseValue)
			.WithHitCount(x + DynamicVars["Hits"].IntValue).FromCard(this, cardPlay)
			.TargetingRandomOpponents(combatState).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card == this && !CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(1m);
		DynamicVars["Hits"].UpgradeValueBy(1m);
	}
}
