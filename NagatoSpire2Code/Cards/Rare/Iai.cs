using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class Iai() : NagatoCardModel(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(60m, ValueProp.Move), new CardsVar(1)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		await CardPileCmd.Add(this, PileType.Draw, CardPilePosition.Random);
		if (!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead && Owner.PlayerCombatState == state)
			await PowerCmd.Apply<IaiDrawPower>(choiceContext, Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != this || Owner.PlayerCombatState?.Phase is not (PlayerTurnPhase.AutoPostPlay or PlayerTurnPhase.End) ||
			CombatState is not { } combatState || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, null)
			.TargetingAllOpponents(combatState)
			.WithHitFx("vfx/vfx_attack_slash", null, "heavy_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(15m);
}
