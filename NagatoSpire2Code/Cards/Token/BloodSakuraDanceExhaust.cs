using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class BloodSakuraDanceExhaust : NagatoChoiceOption
{
	public CardModel SourceCard { get; set; } = null!;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

	public override async Task OnChosen(PlayerChoiceContext choiceContext)
	{
		if (!SourceCard.HasBeenRemovedFromState && SourceCard.Pile is { IsCombatPile: true, Type: not PileType.Exhaust })
			await CardCmd.Exhaust(choiceContext, SourceCard);
	}

	protected override void OnUpgrade() { }
}
