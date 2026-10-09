using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class SakuraBlizzard() : NagatoCardModel(1, CardType.Attack, CardRarity.Common, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DamageVar(7m, ValueProp.Move)];

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != this || Owner.PlayerCombatState is null || CombatState is not { } combatState ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || combatState.HittableEnemies.Count == 0)
			return;

		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, null)
			.TargetingAllOpponents(combatState)
			.WithHitFx("vfx/vfx_attack_slash", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(1m);
		DynamicVars.Damage.UpgradeValueBy(2m);
	}
}
