using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Commands;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class AllOrNothingArmor() : NagatoCardModel(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<PlatingPower>(5m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, DynamicVars["PlatingPower"].BaseValue, Owner.Creature, this);
		while (!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead &&
			Owner.Creature.GetPower<PlatingPower>() is { Amount: > 0 })
		{
			await NagatoPlatingCmd.Trigger(choiceContext, Owner);
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead ||
				Owner.Creature.GetPower<PlatingPower>() is not { Amount: > 0 } current)
				break;

			int amount = current.Amount;
			await PowerCmd.ModifyAmount(choiceContext, current, -1m, null, null);
			if (Owner.Creature.GetPower<PlatingPower>() == current && current.Amount >= amount)
				break;
		}
	}

	protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}
