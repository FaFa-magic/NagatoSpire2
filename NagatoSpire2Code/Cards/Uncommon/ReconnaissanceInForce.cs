using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Keywords;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class ReconnaissanceInForce() : NagatoCardModel(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice, CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move), new CardsVar(1)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<ReconnaissanceInForceSearch>(),
		HoverTipFactory.FromCard<ReconnaissanceInForceDraw>()
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combatState || Owner.PlayerCombatState is not { } state ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
			.FromCard(this, cardPlay)
			.TargetingAllOpponents(combatState)
			.WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
			.Execute(choiceContext);
		if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState != state)
			return;

		CardModel[] options =
		[
			combatState.CreateCard<ReconnaissanceInForceSearch>(Owner),
			combatState.CreateCard<ReconnaissanceInForceDraw>(Owner)
		];
		await NagatoChoiceCmd.Choose(choiceContext, Owner, options);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
