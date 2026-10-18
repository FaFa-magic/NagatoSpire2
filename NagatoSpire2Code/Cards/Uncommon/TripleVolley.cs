using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class TripleVolley() : NagatoCardModel(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

	protected override IEnumerable<DynamicVar> CanonicalVars =>
		[new PowerVar<StrengthPower>(1m), new PowerVar<DexterityPower>(1m), new PowerVar<FocusPower>(1m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<DexterityPower>(), HoverTipFactory.FromPower<FocusPower>()];

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != this)
			return;
		await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars["DexterityPower"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<FocusPower>(choiceContext, Owner.Creature, DynamicVars["FocusPower"].BaseValue, Owner.Creature, this);
	}

	public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
	{
		if (card != this)
			return;
		await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, -DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
		await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, -DynamicVars["DexterityPower"].BaseValue, Owner.Creature, this);
		if (!IsUpgraded)
			await PowerCmd.Apply<FocusPower>(choiceContext, Owner.Creature, -DynamicVars["FocusPower"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade() { }
}
