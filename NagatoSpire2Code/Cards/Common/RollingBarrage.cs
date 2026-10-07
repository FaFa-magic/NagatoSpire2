using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class RollingBarrage() : NagatoCardModel(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CalculationBaseVar(12m),
		new ExtraDamageVar(3m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => NagatoTemporaryPowers.Count(card.Owner.Creature))
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.TemporaryPower];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.CalculatedDamage)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(1m);
}
