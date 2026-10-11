using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class SoulbindingTorii() : NagatoCardModel(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new CalculationBaseVar(0m),
		new ExtraDamageVar(5m),
		new CalculatedDamageVar(ValueProp.Move).WithMultiplier((card, _) => CountSealedCards(card))
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromAffliction<Sealed>();

	protected override IEnumerable<string> ExtraRunAssetPaths =>
		[.. base.ExtraRunAssetPaths, NSealedCardVfx.ScenePath, .. NagatoSoulbindingToriiVfx.AssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		if (CombatState is not { } combatState || Owner.PlayerCombatState is not { } state ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		foreach (var card in state.Hand.Cards.ToArray())
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead ||
				Owner.PlayerCombatState != state || CombatState != combatState)
				return;
			if (card == this || card.Pile != state.Hand || card.Affliction is Sealed)
				continue;
			if (card.Affliction != null)
				CardCmd.ClearAffliction(card);
			await CardCmd.Afflict<Sealed>(card, 1m);
		}

		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead ||
			Owner.PlayerCombatState != state || CombatState != combatState)
			return;
		await DamageCmd.Attack(DynamicVars.CalculatedDamage)
			.FromCard(this, cardPlay)
			.Targeting(cardPlay.Target)
			.BeforeDamage(() => NagatoSoulbindingToriiVfx.Play(Owner.Creature, cardPlay.Target, CountSealedCards(this)))
			.Execute(choiceContext);
	}

	private static int CountSealedCards(CardModel source)
	{
		if (source.Owner.PlayerCombatState is not { } state)
			return 0;
		return state.AllCards.Count(card => card.Affliction is Sealed ||
			source.Pile == state.Hand && card.Pile == state.Hand && card != source);
	}

	protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(2m);
}
