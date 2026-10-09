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

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class ThreefoldTactics() : NagatoCardModel(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
	public override bool GainsBlock => true;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
	[
		new DamageVar(6m, ValueProp.Move),
		new BlockVar(5m, ValueProp.Move),
		new RepeatVar(3)
	];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<ThreefoldTacticsAttack>(IsUpgraded),
		HoverTipFactory.FromCard<ThreefoldTacticsBlock>(IsUpgraded)
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		if (CombatState is not { } combatState || Owner.PlayerCombatState is not { } state ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		var attack = combatState.CreateCard<ThreefoldTacticsAttack>(Owner);
		attack.SourceCard = this;
		attack.SourcePlay = cardPlay;
		var block = combatState.CreateCard<ThreefoldTacticsBlock>(Owner);
		block.SourcePlay = cardPlay;
		CardModel[] options = [attack, block];
		if (IsUpgraded)
			foreach (var option in options)
				CardCmd.Upgrade(option);

		for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState != state)
				break;
			await NagatoChoiceCmd.Choose(choiceContext, Owner, options);
		}
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Damage.UpgradeValueBy(2m);
		DynamicVars.Block.UpgradeValueBy(1m);
	}
}
