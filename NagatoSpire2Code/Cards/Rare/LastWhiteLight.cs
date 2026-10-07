using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class LastWhiteLight() : NagatoCardModel(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<LastWhiteLightPower>(),
		HoverTipFactory.FromPower<LastWhiteLightEchoPower>()
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState == null || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		if (Owner.Creature.GetPower<LastWhiteLightPower>() == null)
			await PowerCmd.Apply<LastWhiteLightPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
		if (Owner.Creature.GetPower<LastWhiteLightPower>() != null && !CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await PowerCmd.Apply<LastWhiteLightEchoPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
	}

	protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
