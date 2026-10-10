using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class RangingFire() : NagatoCardModel(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	protected override IEnumerable<DynamicVar> CanonicalVars =>
		[new DamageVar(14m, ValueProp.Move), new PowerVar<VulnerablePower>(2m), new PowerVar<LockOnPower>(2m)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[HoverTipFactory.FromPower<VulnerablePower>(), HoverTipFactory.FromPower<LockOnPower>()];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || !cardPlay.Target!.IsHittable)
			return;
		await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<LockOnPower>(choiceContext, cardPlay.Target, DynamicVars["LockOnPower"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(4m);
		DynamicVars["VulnerablePower"].UpgradeValueBy(1m);
		DynamicVars["LockOnPower"].UpgradeValueBy(1m);
	}
}
