using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class SakuraForm() : NagatoCardModel(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<SakuraFormPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Ethereal);
	}
}
