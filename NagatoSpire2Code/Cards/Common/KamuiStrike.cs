using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class KamuiStrike() : NagatoCardModel(1, CardType.Attack, CardRarity.Common, TargetType.Self)
{
	protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
	
	public override HashSet<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(8m, ValueProp.Move),
		new PowerVar<KamuiStrikeStrengthPower>(2m)
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<KamuiStrikeStrengthPower>()];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await PowerCmd.Apply<KamuiStrikeStrengthPower>(choiceContext, Owner.Creature,
			DynamicVars["KamuiStrikeStrengthPower"].BaseValue, Owner.Creature, this);

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != this || Owner.PlayerCombatState is null || CombatState is not { } combatState ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || combatState.HittableEnemies.Count == 0)
			return;

		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, null)
			.TargetingRandomOpponents(combatState)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(choiceContext);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(3m);
		DynamicVars["KamuiStrikeStrengthPower"].UpgradeValueBy(1m);
	}
}
