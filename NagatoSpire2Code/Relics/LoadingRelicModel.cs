using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Relics;

public abstract class LoadingRelicModel : NagatoRelicModel
{
	protected virtual OrbModel? ShellOrb => null;

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[NagatoHoverTips.Loading, HoverTipFactory.FromCard<Shell>(), .. ShellOrb?.HoverTips ?? []];

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
		IEnumerable<Creature> participants)
	{
		if (side != CombatSide.Player || !participants.Contains(Owner.Creature) || Owner.Creature.IsDead ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.CombatState is not { } combat)
			return;
		Flash();
		var card = combat.CreateCard<Shell>(Owner);
		if (ShellOrb is { } orb)
			await NagatoCardOrbs.Bind(choiceContext, card, orb.ToMutable());
		await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Play, Owner);
		await NagatoCardSlots.Load(choiceContext, card);
	}
}
