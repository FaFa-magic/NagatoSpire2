using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Keywords;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class ReturnToFormation() : NagatoCardModel(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	private bool _returnAfterPlay;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice, CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(1), new CardsVar(1)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<PlatingPower>(),
		HoverTipFactory.FromCard<ReturnToFormationEvoke>(IsUpgraded),
		HoverTipFactory.FromCard<ReturnToFormationDraw>(IsUpgraded)
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combatState || Owner.PlayerCombatState == null ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		CardModel[] options =
		[
			combatState.CreateCard<ReturnToFormationEvoke>(Owner),
			combatState.CreateCard<ReturnToFormationDraw>(Owner)
		];
		if (IsUpgraded)
			foreach (var option in options)
				CardCmd.Upgrade(option);

		await NagatoChoiceCmd.Choose(choiceContext, Owner, options);
	}

	public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
		decimal amount, Creature? applier, CardModel? cardSource)
	{
		if (power is not PlatingPower || power.Owner != Owner.Creature || amount <= 0m ||
			CombatState == null || Pile?.IsCombatPile != true ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		if (CombatManager.Instance.History.Entries.OfType<PowerReceivedEntry>()
			.Where(entry => entry.Actor == Owner.Creature && entry.Power is PlatingPower &&
				entry.Amount > 0m && entry.HappenedThisTurn(CombatState)).Take(2).Count() != 1)
			return;

		if (Pile.Type == PileType.Play)
			_returnAfterPlay = true;
		else if (Pile.Type != PileType.Hand)
			await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom);
	}

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card != this || !_returnAfterPlay)
			return;

		_returnAfterPlay = false;
		if (Pile?.Type == PileType.Exhaust && !CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await CardPileCmd.Add(this, PileType.Hand, CardPilePosition.Bottom);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Repeat.UpgradeValueBy(1m);
		DynamicVars.Cards.UpgradeValueBy(1m);
	}
}
