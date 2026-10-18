using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class SakuraFormPower : NagatoPowerModel
{
	private sealed class Data
	{
		public int AttacksDrawn;
	}

	protected override bool UsesPackedIcon => true;
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override object InitInternalData() => new Data();
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading];

	public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side,
		IReadOnlyList<Creature> participants, ICombatState combatState)
	{
		if (side == CombatSide.Player && participants.Contains(Owner))
			GetInternalData<Data>().AttacksDrawn = 0;
		return Task.CompletedTask;
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card.Owner != Owner.Player || card.Type != CardType.Attack || card.HasBeenRemovedFromState || Owner.IsDead ||
			CombatManager.Instance.IsOverOrEnding || Owner.Player is not { PlayerCombatState: { } state } player)
			return;
		var data = GetInternalData<Data>();
		if (data.AttacksDrawn >= Amount)
			return;
		data.AttacksDrawn++;
		Flash();
		await NagatoCardSlots.Load(choiceContext, card);
		if (!Owner.IsDead && !CombatManager.Instance.IsOverOrEnding && player.PlayerCombatState == state)
			await CardPileCmd.Draw(choiceContext, 1, player);
	}
}
