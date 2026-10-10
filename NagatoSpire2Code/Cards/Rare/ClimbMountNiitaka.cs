using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class ClimbMountNiitaka() : NagatoCardModel(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState == null)
			return;

		await PowerCmd.Apply<ClimbMountNiitakaPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
		await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
	}

	protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
