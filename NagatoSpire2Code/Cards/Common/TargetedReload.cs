using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Orbs;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Keywords;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using NagatoSpire2.NagatoSpire2Code.Patches;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class TargetedReload() : NagatoCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	public override bool GainsBlock => true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Load];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding)
			return;

		OrbQueue queue = state.OrbQueue;
		if (queue.Capacity == 0)
			return;

		OrbModel[] choices = queue.Orbs.ToArray();
		int? selectedIndex = await NagatoOrbSelectCmd.Select(choiceContext, Owner, choices);
		if (CombatManager.Instance.IsOverOrEnding)
			return;

		NagatoOrbResolutionScope.Enter(Owner);
		try
		{
			if (selectedIndex is int index && index >= 0 && index < choices.Length && queue.Orbs.Contains(choices[index]))
			{
				await OrbCmd.EvokeNext(choiceContext, Owner);
				if (CombatManager.Instance.IsOverOrEnding)
					return;

				OrbModel shell = NagatoShellOrb.CreateRandom(Owner);
				await OrbCmd.Channel(choiceContext, shell, Owner);
				if (queue.Remove(shell))
				{
					queue.Insert(Math.Min(index, queue.Orbs.Count), shell);
					NagatoOrbSelectCmd.SyncVisualOrder(Owner);
				}
			}
			else
			{
				await OrbCmd.Channel(choiceContext, NagatoShellOrb.CreateRandom(Owner), Owner);
			}
		}
		finally
		{
			await NagatoOrbResolutionScope.Exit(Owner, choiceContext);
		}
	}

	protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
