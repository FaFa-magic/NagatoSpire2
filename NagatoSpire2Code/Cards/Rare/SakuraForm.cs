using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class SakuraForm() : NagatoCardModel(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SakuraFormPower>(3m)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await PowerCmd.Apply<SakuraFormPower>(choiceContext, Owner.Creature,
			DynamicVars["SakuraFormPower"].BaseValue, Owner.Creature, this);

	protected override void OnUpgrade() { }
}
