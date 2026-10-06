using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class ReincarnationsEndPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Debuff;
	public override PowerStackType StackType => PowerStackType.Single;
	public override bool ShouldPlayVfx => false;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

	public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
	{
		if (player != Owner.Player)
			return;

		Flash();
		var hand = player.PlayerCombatState?.Hand;
		var cards = hand?.Cards.ToArray() ?? [];
		await PowerCmd.Remove(this);
		foreach (var card in cards)
		{
			if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead)
				break;
			if (card.Pile == hand)
				await CardCmd.Exhaust(choiceContext, card);
		}
	}
}
