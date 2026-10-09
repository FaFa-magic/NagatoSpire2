using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Keywords;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class ShellSelection() : NagatoCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<ShellSelectionHighExplosive>(),
		HoverTipFactory.FromCard<ShellSelectionArmorPiercing>(),
		HoverTipFactory.FromCard<ShellSelectionTypeThree>()
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combatState || Owner.PlayerCombatState == null ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		CardModel[] options =
		[
			combatState.CreateCard<ShellSelectionHighExplosive>(Owner),
			combatState.CreateCard<ShellSelectionArmorPiercing>(Owner),
			combatState.CreateCard<ShellSelectionTypeThree>(Owner)
		];
		await NagatoChoiceCmd.Choose(choiceContext, Owner, options);
	}

	protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
