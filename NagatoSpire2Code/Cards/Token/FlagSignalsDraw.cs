using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class FlagSignalsDraw : NagatoFleetChoiceOption
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

	public override Task ApplyTo(PlayerChoiceContext choiceContext, Player player, CardModel? source) =>
		CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, player);

	protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
