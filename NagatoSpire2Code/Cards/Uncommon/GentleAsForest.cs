using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class GentleAsForest() : NagatoCardModel(3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

	protected override void OnUpgrade() { }
}
