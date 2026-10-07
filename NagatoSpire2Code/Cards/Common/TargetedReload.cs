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
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using NagatoSpire2.NagatoSpire2Code.Patches;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class TargetedReload() : NagatoCardModel(1, CardType.Skill, CardRarity.Common, TargetType.TargetedNoCreature)
{
	private OrbModel? _pendingOrbTarget;

	public override bool GainsBlock => true;
	public override CardAssetProfile AssetProfile => base.AssetProfile with
	{
		PortraitPath = "res://NagatoSpire2/images/cards/Defend.png"
	};

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Load];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];

	public void SetOrbTarget(OrbModel? orb) => _pendingOrbTarget = orb;

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		OrbModel? target = _pendingOrbTarget;
		_pendingOrbTarget = null;
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding)
			return;

		OrbQueue queue = state.OrbQueue;
		int? selectedIndex = await ResolveTarget(choiceContext, cardPlay, queue, target);
		if (CombatManager.Instance.IsOverOrEnding)
			return;
		OrbModel? selectedOrb = selectedIndex is int slot && slot >= 0 && slot < queue.Orbs.Count
			? queue.Orbs[slot]
			: null;
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		if (queue.Capacity == 0 || CombatManager.Instance.IsOverOrEnding)
			return;
		int index = selectedOrb == null ? -1 : queue.Orbs.ToList().IndexOf(selectedOrb);
		var prefix = queue.Orbs.Take(index + 1).ToHashSet();
		var orbManager = index >= 0 ? NagatoOrbTargetingPatches.DeferLayout(Owner) : null;

		NagatoOrbResolutionScope.Enter(Owner);
		try
		{
			if (index >= 0)
			{
				await OrbCmd.EvokeNext(choiceContext, Owner);
				if (CombatManager.Instance.IsOverOrEnding)
					return;

				OrbModel shell = NagatoShellOrb.CreateRandom(Owner);
				await OrbCmd.Channel(choiceContext, shell, Owner);
				if (queue.Remove(shell))
				{
					queue.Insert(queue.Orbs.Count(prefix.Contains), shell);
				}
			}
			else
			{
				await OrbCmd.Channel(choiceContext, NagatoShellOrb.CreateRandom(Owner), Owner);
			}
		}
		finally
		{
			try
			{
				await NagatoOrbResolutionScope.Exit(Owner, choiceContext);
			}
			finally
			{
				NagatoOrbTargetingPatches.ResumeLayout(orbManager, Owner);
			}
		}
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
