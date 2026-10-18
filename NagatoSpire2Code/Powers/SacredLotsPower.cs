using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class SacredLotsPower : NagatoPowerModel
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[NagatoHoverTips.Residence, .. HoverTipFactory.FromAffliction<Sealed>()];

	public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
		IEnumerable<Creature> participants)
	{
		if (!participants.Contains(Owner) || Owner.IsDead || CombatManager.Instance.IsOverOrEnding ||
			Owner.Player is not { PlayerCombatState: { } state } player)
			return;
		int repeats = Amount;
		for (int i = 0; i < repeats; i++)
		{
			if (Owner.IsDead || CombatManager.Instance.IsOverOrEnding || player.PlayerCombatState != state)
				break;
			var cards = CardFactory.GetDistinctForCombat(player,
				player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint),
				3, player.RunState.Rng.CombatCardGeneration).ToArray();
			if (cards.Length == 0)
				break;
			var orbs = cards.Select(_ => NagatoOrbPool.CreateRandom(player.RunState.Rng.CombatOrbGeneration)).ToArray();
			var previews = cards.Select((card, index) =>
			{
				var preview = (CardModel)card.MutableClone();
				preview.GetOrCreateCapability<CardOrbCapability>().SetOrbs(
					[.. NagatoCardOrbs.Get(preview).TakeLast(CardOrbCapability.MaxOrbs - 1), (OrbModel)orbs[index].MutableClone()]);
				return preview;
			}).ToArray();
			Flash();
			var selected = await CardSelectCmd.FromChooseACardScreen(choiceContext, previews, player, canSkip: true);
			if (Owner.IsDead || CombatManager.Instance.IsOverOrEnding || player.PlayerCombatState != state)
				break;
			int selectedIndex = Array.IndexOf(previews, selected);
			if (selectedIndex < 0)
				continue;
			var chosen = cards[selectedIndex];
			await NagatoCardOrbs.Bind(choiceContext, chosen, orbs[selectedIndex]);
			if (Owner.IsDead || CombatManager.Instance.IsOverOrEnding || player.PlayerCombatState != state)
				break;
			if (chosen.Affliction is not Sealed)
			{
				CardCmd.ClearAffliction(chosen);
				await CardCmd.Afflict<Sealed>(chosen, 1m);
			}
			await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Draw, player, CardPilePosition.Top);
		}
	}
}
