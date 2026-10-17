using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class ShadowReturn : NagatoChoiceOption
{
	public CardModel SourceCard { get; set; } = null!;

	public override async Task OnChosen(PlayerChoiceContext choiceContext)
	{
		if (!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead && SourceCard.Owner == Owner &&
			!SourceCard.HasBeenRemovedFromState && SourceCard.Pile?.IsCombatPile == true)
			await CardPileCmd.Add(SourceCard, PileType.Draw, CardPilePosition.Top);
	}

	protected override void OnUpgrade() { }
}
