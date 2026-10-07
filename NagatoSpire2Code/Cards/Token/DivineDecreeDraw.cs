using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class DivineDecreeDraw : NagatoChoiceOption
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

	public override Task OnChosen(PlayerChoiceContext choiceContext) => CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

	protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
