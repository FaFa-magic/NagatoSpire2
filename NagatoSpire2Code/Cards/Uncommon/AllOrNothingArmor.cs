using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Commands;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class AllOrNothingArmor() : NagatoCardModel(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PlatingPower>(5m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, DynamicVars["PlatingPower"].BaseValue, Owner.Creature, this);
		while (!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead &&
			Owner.Creature.GetPower<PlatingPower>() is { Amount: > 0 })
			await NagatoPlatingCmd.Trigger(choiceContext, Owner);
	}

	protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
