using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class PiercingOnslaught() : NagatoCardModel(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

	protected override void OnUpgrade() { }
}
