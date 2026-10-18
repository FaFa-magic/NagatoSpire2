using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class UndyingEmbers() : NagatoCardModel(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9m, ValueProp.Move)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combat || Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)
			.TargetingAllOpponents(combat).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || CombatState != combat)
			return;
		CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(CreateClone(), PileType.Discard, Owner), 2.2f);
	}

	public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
	{
		if (side == CombatSide.Player && participants.Contains(Owner.Creature) && Pile?.Type == PileType.Exhaust &&
			!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await NagatoCardSlots.Load(choiceContext, this);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
