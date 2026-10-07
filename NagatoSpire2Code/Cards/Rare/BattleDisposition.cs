using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using NagatoSpire2.NagatoSpire2Code.Commands;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class BattleDisposition() : NagatoCardModel(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await NagatoCardOrderCmd.ReorderDrawPile(choiceContext, Owner, SelectionScreenPrompt);

	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
