using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class TwinMountStrike() : NagatoCardModel(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new DamageVar(2m, ValueProp.Move), 
		new IntVar("Hits", 2)
	];
	
	protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
	
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars["Hits"].IntValue)
			.FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
		if (Owner.PlayerCombatState is { } state && !CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await OrbCmd.EvokeLast(choiceContext, Owner);
	}

	protected override void OnUpgrade() => DynamicVars["Hits"].UpgradeValueBy(1m);
}
