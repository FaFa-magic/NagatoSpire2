using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Commands;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class ReturnToBloom() : NagatoCardModel(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
	private bool _returnToHand;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.IsFirstInSeries)
			_returnToHand = false;
		int before = NagatoOrbHistory.GetEvokeCount(Owner);
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
		if (Owner.PlayerCombatState is { } state && !CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead &&
			Owner.RunState.Rng.CombatCardSelection.NextItem(state.OrbQueue.Orbs) is { } orb)
			await NagatoOrbCmd.Evoke(choiceContext, Owner, [orb]);
		_returnToHand |= NagatoOrbHistory.GetEvokeCount(Owner) - before >= 3;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card == this && cardPlay.IsLastInSeries && _returnToHand &&
			cardPlay.ResultPile != PileType.Exhaust && Pile?.Type == PileType.Play &&
			!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await CardPileCmd.Add(this, PileType.Hand);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
