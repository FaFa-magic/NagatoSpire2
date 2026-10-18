using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Multiplay;

public sealed class CircularFormation() : NagatoCardModel(2, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PlatingPower>(6m)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combat)
			return;
		var players = combat.GetTeammatesOf(Owner.Creature)
			.Where(creature => creature.IsPlayer && creature.IsAlive).ToArray();
		await PowerCmd.Apply<PlatingPower>(choiceContext, players, DynamicVars["PlatingPower"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade() => DynamicVars["PlatingPower"].UpgradeValueBy(2m);
}
