using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using NagatoSpire2.NagatoSpire2Code.Cards;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class SakuraForm() : NagatoCardModel(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
	public override CardAssetProfile AssetProfile => base.AssetProfile with
	{
		PortraitPath = "res://NagatoSpire2/images/background/sakura_tree.png"
	};

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<SakuraFormPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
	}

	protected override void OnUpgrade()
	{
		RemoveKeyword(CardKeyword.Ethereal);
	}
}
