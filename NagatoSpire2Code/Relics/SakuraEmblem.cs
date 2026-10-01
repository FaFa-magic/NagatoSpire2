using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Characters;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using NagatoSpire2.NagatoSpire2Code.Patches;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NagatoSpire2.NagatoSpire2Code.Relics;

[RegisterCharacterStarterRelic(typeof(NagatoCharacter))]
public sealed class SakuraEmblem : NagatoRelicModel
{
	private bool _isRefilling;

	public override RelicRarity Rarity => RelicRarity.Starter;

	public override async Task BeforeSideTurnStart(
		PlayerChoiceContext choiceContext,
		CombatSide side,
		IReadOnlyList<Creature> participants,
		ICombatState combatState)
	{
		if (participants.Contains(Owner.Creature) && Owner.PlayerCombatState!.TurnNumber <= 1)
			await FillEmptySlots(choiceContext);
	}

	public override async Task AfterOrbEvoked(
		PlayerChoiceContext choiceContext,
		OrbModel orb,
		IEnumerable<Creature> targets)
	{
		if (orb.Owner != Owner || NagatoOrbResolutionScope.DeferRefill(Owner))
			return;

		await FillEmptySlots(choiceContext);
	}

	public async Task FillEmptySlots(PlayerChoiceContext choiceContext)
	{
		if (_isRefilling || Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;

		var queue = Owner.PlayerCombatState!.OrbQueue;
		if (queue.Orbs.Count >= queue.Capacity)
			return;

		_isRefilling = true;
		try
		{
			Flash();
			OrbModel[] shells =
			[
				ModelDb.Orb<HighExplosiveShellOrb>(),
				ModelDb.Orb<ArmorPiercingShellOrb>(),
				ModelDb.Orb<TypeThreeShellOrb>()
			];
			while (queue.Orbs.Count < queue.Capacity && !CombatManager.Instance.IsOverOrEnding)
			{
				OrbModel shell = Owner.RunState.Rng.CombatOrbGeneration.NextItem(shells)!;
				int count = queue.Orbs.Count;
				await OrbCmd.Channel(choiceContext, shell.ToMutable(), Owner);
				if (queue.Orbs.Count <= count)
					break;
			}
		}
		finally
		{
			_isRefilling = false;
		}
	}
}
