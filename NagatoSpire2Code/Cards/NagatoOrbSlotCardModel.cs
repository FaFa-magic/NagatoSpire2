using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace NagatoSpire2.NagatoSpire2Code.Cards;

public abstract class NagatoOrbSlotCardModel(
	int energyCost,
	CardType type,
	CardRarity rarity,
	bool shouldShowInCardLibrary = true)
	: NagatoCardModel(energyCost, type, rarity, TargetType.TargetedNoCreature, shouldShowInCardLibrary)
{
	public int? OrbSlotIndex { get; private set; }

	public void SetOrbSlotTarget(int? index) => OrbSlotIndex = index;

	protected override void AfterCloned()
	{
		base.AfterCloned();
		OrbSlotIndex = null;
	}

	protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		int? target = OrbSlotIndex;
		OrbSlotIndex = null;
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		OrbSlotIndex = await ResolveTarget(choiceContext, cardPlay, state.OrbQueue, target);
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState != state)
			return;

		await OnPlayWithOrbSlot(choiceContext, cardPlay, OrbSlotIndex);
	}

	protected abstract Task OnPlayWithOrbSlot(PlayerChoiceContext choiceContext, CardPlay cardPlay, int? slotIndex);

	private async Task<int?> ResolveTarget(PlayerChoiceContext choiceContext, CardPlay cardPlay, OrbQueue queue, int? target)
	{
		if (cardPlay.IsAutoPlay)
			return null;

		uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(Owner);
		await choiceContext.SignalPlayerChoiceBegun(Owner, PlayerChoiceOptions.None);
		try
		{
			if (LocalContext.IsMe(Owner) && RunManager.Instance.NetService.Type != NetGameType.Replay)
			{
				int index = target is int slot && slot >= 0 && slot < queue.Capacity ? slot : -1;
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromIndex(index));
				return index >= 0 ? index : null;
			}

			int remoteIndex = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(Owner, choiceId)).AsIndex();
			return remoteIndex >= 0 && remoteIndex < queue.Capacity ? remoteIndex : null;
		}
		finally
		{
			await choiceContext.SignalPlayerChoiceEnded();
		}
	}
}
