using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class RemainingMomentum() : NagatoCardModel(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [EnergyHoverTip];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combat || Owner.PlayerCombatState == null ||
			Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		int spent = CombatManager.Instance.History.Entries.OfType<EnergySpentEntry>()
			.Where(entry => entry.HappenedThisTurn(combat) && entry.Actor.Player == Owner).Sum(entry => entry.Amount);
		await PlayerCmd.GainEnergy(Math.Max(0, spent - cardPlay.Resources.EnergySpent), Owner);
	}

	protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
