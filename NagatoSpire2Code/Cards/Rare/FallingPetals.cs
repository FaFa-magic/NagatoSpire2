using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class FallingPetals() : NagatoCardModel(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CalculationBaseVar(0m),
		new ExtraDamageVar(1m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => NagatoTemporaryPowers.Count(card.Owner.Creature)),
		new IntVar("Reduction", 1)
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.TemporaryPower];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		await DamageCmd.Attack(DynamicVars.CalculatedDamage)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash", null, "heavy_attack.mp3")
			.Execute(choiceContext);
		await NagatoTemporaryPowers.Reduce(choiceContext, Owner.Creature, DynamicVars["Reduction"].IntValue, this);
	}

	protected override void OnUpgrade() => DynamicVars["Reduction"].UpgradeValueBy(1m);
}
