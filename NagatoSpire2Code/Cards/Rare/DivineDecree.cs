using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Keywords;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class DivineDecree() : NagatoCardModel(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice, CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2), new CardsVar(3)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<DivineDecreeEnergy>(IsUpgraded),
		HoverTipFactory.FromCard<DivineDecreeDraw>(IsUpgraded)
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combatState || Owner.PlayerCombatState == null ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		CardModel[] options =
		[
			combatState.CreateCard<DivineDecreeEnergy>(Owner),
			combatState.CreateCard<DivineDecreeDraw>(Owner)
		];
		if (IsUpgraded)
			foreach (var option in options)
				CardCmd.Upgrade(option);

		await NagatoChoiceCmd.Choose(choiceContext, Owner, options);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Energy.UpgradeValueBy(1m);
		DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
