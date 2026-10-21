using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class DivineDecreeGuard : NagatoChoiceOption
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<DivineDecreeGuardPower>()];

	public override async Task OnChosen(PlayerChoiceContext choiceContext) =>
		await PowerCmd.Apply<DivineDecreeGuardPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

	protected override void OnUpgrade() { }
}
