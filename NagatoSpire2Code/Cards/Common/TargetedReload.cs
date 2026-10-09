using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class TargetedReload() : NagatoCardModel(1, CardType.Skill, CardRarity.Common, TargetType.TargetedNoCreature)
{
	private OrbModel? _pendingOrbTarget;

	public override bool GainsBlock => true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		NagatoHoverTips.Load,
		HoverTipFactory.FromCard<TargetedReloadHighExplosive>(),
		HoverTipFactory.FromCard<TargetedReloadArmorPiercing>()
	];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7m, ValueProp.Move)];

	public void SetOrbTarget(OrbModel? orb) => _pendingOrbTarget = orb;

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		OrbModel? target = _pendingOrbTarget;
		_pendingOrbTarget = null;
		if (Owner.PlayerCombatState is not { } state || CombatState is not { } combatState ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		OrbQueue queue = state.OrbQueue;
		int? selectedIndex = await ResolveTarget(choiceContext, cardPlay, queue, target);
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState != state)
			return;
		OrbModel? selectedOrb = selectedIndex is int slot && slot >= 0 && slot < queue.Orbs.Count
			? queue.Orbs[slot]
			: null;
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		if (queue.Capacity == 0 || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead ||
			Owner.PlayerCombatState != state)
			return;
		int index = selectedOrb == null ? -1 : queue.Orbs.ToList().IndexOf(selectedOrb);
		var highExplosive = combatState.CreateCard<TargetedReloadHighExplosive>(Owner);
		highExplosive.SlotIndex = index >= 0 ? index : null;
		var armorPiercing = combatState.CreateCard<TargetedReloadArmorPiercing>(Owner);
		armorPiercing.SlotIndex = highExplosive.SlotIndex;
		await NagatoChoiceCmd.Choose(choiceContext, Owner, [highExplosive, armorPiercing]);
	}

	private async Task<int?> ResolveTarget(PlayerChoiceContext choiceContext, CardPlay cardPlay, OrbQueue queue, OrbModel? target)
	{
		if (cardPlay.IsAutoPlay)
			return null;

		uint choiceId = RunManager.Instance.PlayerChoiceSynchronizer.ReserveChoiceId(Owner);
		await choiceContext.SignalPlayerChoiceBegun(Owner, PlayerChoiceOptions.None);
		try
		{
			if (LocalContext.IsMe(Owner) && RunManager.Instance.NetService.Type != NetGameType.Replay)
			{
				int index = target == null ? -1 : queue.Orbs.ToList().IndexOf(target);
				RunManager.Instance.PlayerChoiceSynchronizer.SyncLocalChoice(Owner, choiceId, PlayerChoiceResult.FromIndex(index));
				return index >= 0 ? index : null;
			}

			int remoteIndex = (await RunManager.Instance.PlayerChoiceSynchronizer.WaitForRemoteChoice(Owner, choiceId)).AsIndex();
			return remoteIndex >= 0 && remoteIndex < queue.Orbs.Count ? remoteIndex : null;
		}
		finally
		{
			await choiceContext.SignalPlayerChoiceEnded();
		}
	}

	protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
