using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class SacredTree() : NagatoCardModel(-1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [EnergyHoverTip];

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card == this)
			await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
	}

	protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1m);
}
