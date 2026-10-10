using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Commands;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class ResonantSalvo() : NagatoCardModel(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead ||
			state.OrbQueue.Orbs.FirstOrDefault() is not { } first)
			return;
		await NagatoOrbCmd.Evoke(choiceContext, Owner, state.OrbQueue.Orbs.Where(orb => orb.GetType() == first.GetType()).ToArray());
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
