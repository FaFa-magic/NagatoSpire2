using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class DivineDecreeWrath : NagatoChoiceOption
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<DivineDecreeWrathPower>()];

	public override async Task OnChosen(PlayerChoiceContext choiceContext) =>
		await PowerCmd.Apply<DivineDecreeWrathPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

	protected override void OnUpgrade() { }
}
