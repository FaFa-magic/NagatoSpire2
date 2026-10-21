using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class BigSeven() : NagatoCardModel(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading, NagatoHoverTips.Salvo, HoverTipFactory.FromCard<Shell>()];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. ModelDb.Card<Shell>().RunAssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await PowerCmd.Apply<BigSevenPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

	protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
