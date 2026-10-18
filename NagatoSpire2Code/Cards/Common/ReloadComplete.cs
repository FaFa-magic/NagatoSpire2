using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class ReloadComplete() : NagatoCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading, HoverTipFactory.FromCard<Shell>()];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. ModelDb.Card<Shell>().RunAssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatState is not { } combat ||
			Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		int count = NagatoCardSlots.GetEmptySlotCount(Owner);
		for (int i = 0; i < count; i++)
		{
			if (Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding || Owner.PlayerCombatState != state)
				break;
			var shell = combat.CreateCard<Shell>(Owner);
			await CardPileCmd.AddGeneratedCardToCombat(shell, PileType.Play, Owner);
			await NagatoCardSlots.Load(choiceContext, shell);
		}
	}

	protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
