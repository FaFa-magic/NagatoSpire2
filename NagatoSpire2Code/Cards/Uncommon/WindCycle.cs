using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using STS2RitsuLib;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class WindCycle() : NagatoCardModel(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Orbs", 3m)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Residence];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;
		while (Owner.PlayerCombatState == state && !CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead &&
			state.Hand.Cards.Count < RitsuLibFramework.GetMaxHandSize(Owner) &&
			state.Hand.Cards.Sum(card => NagatoCardOrbs.Get(card).Count) < DynamicVars["Orbs"].IntValue)
			if (!(await CardPileCmd.Draw(choiceContext, 1, Owner)).Any())
				break;
	}

	protected override void OnUpgrade() => DynamicVars["Orbs"].UpgradeValueBy(1m);
}
