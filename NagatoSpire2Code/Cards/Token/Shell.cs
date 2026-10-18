using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Characters;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

[RegisterCard(typeof(TokenCardPool))]
public sealed class Shell() : NagatoCardModel(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<NagatoCardPool>();

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move), new PowerVar<WeakPower>(1m)];

	private bool HasGunneryManual => IsMutable && Owner?.Creature.HasPower<GunneryManualPower>() == true;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		HasGunneryManual ? [HoverTipFactory.FromPower<WeakPower>()] : [];

	protected override void AddExtraArgsToDescription(LocString description) =>
		description.Add("GunneryManual", HasGunneryManual);

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt")
			.Execute(choiceContext);
		if (HasGunneryManual && !CombatManager.Instance.IsOverOrEnding && cardPlay.Target.IsHittable)
			await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target,
				DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
