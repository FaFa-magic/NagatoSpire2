using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class DecisiveBattle() : NagatoCardModel(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading, HoverTipFactory.FromCard<Shell>(IsUpgraded)];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. ModelDb.Card<Shell>().RunAssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatState is not { } combat ||
			Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		int count = state.ExhaustPile.Cards.Count(card => card is Shell);
		for (int i = 0; i < count; i++)
		{
			if (Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding || Owner.PlayerCombatState != state)
				break;
			var shell = combat.CreateCard<Shell>(Owner);
			if (IsUpgraded)
				CardCmd.Upgrade(shell);
			await CardPileCmd.AddGeneratedCardToCombat(shell, PileType.Play, Owner);
			await NagatoCardSlots.Load(choiceContext, shell);
		}
	}

	protected override void OnUpgrade() { }
}
