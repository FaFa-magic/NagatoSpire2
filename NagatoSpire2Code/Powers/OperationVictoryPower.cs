using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class OperationVictoryPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Residence];

	public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
	{
		if (creator?.Creature != Owner || creator.PlayerCombatState is not { } state || Owner.IsDead ||
			CombatManager.Instance.IsOverOrEnding || card.HasBeenRemovedFromState || card.Pile?.IsCombatPile != true)
			return;
		int count = Amount;
		var context = new BlockingPlayerChoiceContext();
		Flash();
		for (int i = 0; i < count; i++)
		{
			if (Owner.IsDead || card.Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding ||
				creator.PlayerCombatState != state || card.HasBeenRemovedFromState || card.Pile?.IsCombatPile != true)
				break;
			await NagatoCardOrbs.Bind(context, card, NagatoOrbPool.CreateRandom(creator.RunState.Rng.CombatOrbGeneration));
		}
	}
}
