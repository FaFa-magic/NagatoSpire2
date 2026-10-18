using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Afflictions;

public sealed class Sealed : NagatoAfflictionModel
{
	private bool _autoplaying;

	public override bool HasExtraCardText => true;

	internal bool IsAutoplaying => _autoplaying;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading, HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

	public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType) =>
		!HasCard || card != Card || autoPlayType != AutoPlayType.None;

	public override bool TryModifyKeywordsInCombat(CardModel card, ISet<CardKeyword> keywords) =>
		HasCard && card == Card && _autoplaying && keywords.Remove(CardKeyword.Unplayable);

	public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources,
		CardLocation cardLocation) =>
		HasCard && card == Card && isAutoPlay && _autoplaying
			? cardLocation with { player = card.Owner, pileType = PileType.Exhaust, position = CardPilePosition.Bottom }
			: cardLocation;

	internal async Task Play(PlayerChoiceContext choiceContext)
	{
		if (!HasCard || _autoplaying || Card.Owner.PlayerCombatState is not { } state ||
			CombatManager.Instance.IsOverOrEnding || Card.Owner.Creature.IsDead)
			return;
		var card = Card;
		_autoplaying = true;
		try
		{
			await CardCmd.AutoPlay(choiceContext, card, null);
			if (!CombatManager.Instance.IsOverOrEnding && !card.Owner.Creature.IsDead &&
				card.Owner.PlayerCombatState == state && !card.HasBeenRemovedFromState &&
				card.Pile is { Type: not PileType.Exhaust })
				await CardCmd.Exhaust(choiceContext, card);
		}
		finally
		{
			_autoplaying = false;
		}
	}

	protected override void AfterCloned()
	{
		base.AfterCloned();
		_autoplaying = false;
	}
}
