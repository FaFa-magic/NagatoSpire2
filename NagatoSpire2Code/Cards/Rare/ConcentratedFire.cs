using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class ConcentratedFire() : NagatoCardModel(1, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(11m, ValueProp.Move)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<FocusPower>(),
		NagatoHoverTips.TemporaryPower
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combatState || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.TargetingAllOpponents(combatState)
			.WithHitFx("vfx/vfx_attack_slash", null, "heavy_attack.mp3")
			.Execute(choiceContext);
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		decimal focus = NagatoTemporaryPowers.Count(Owner.Creature, IsUpgraded ? null : typeof(FocusPower));
		if (focus > 0m)
			await PowerCmd.Apply<ConcentratedFireFocusPower>(choiceContext, Owner.Creature, focus, Owner.Creature, this);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
