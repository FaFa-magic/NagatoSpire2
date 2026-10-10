using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Cards;

public abstract class NagatoFleetChoiceOption : NagatoChoiceOption
{
	public sealed override Task OnChosen(PlayerChoiceContext choiceContext) => ApplyTo(choiceContext, Owner, null);
	public abstract Task ApplyTo(PlayerChoiceContext choiceContext, Player player, CardModel? source);
}
