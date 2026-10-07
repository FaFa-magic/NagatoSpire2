using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using NagatoSpire2.NagatoSpire2Code.Keywords;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class DivineProtection() : NagatoCardModel(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DivineProtectionPlatingPower>(3m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromKeyword(NagatoKeywords.Choice),
		HoverTipFactory.FromPower<DivineProtectionPlatingPower>()
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await PowerCmd.Apply<DivineProtectionPower>(choiceContext, Owner.Creature,
			DynamicVars["DivineProtectionPlatingPower"].BaseValue, Owner.Creature, this);

	protected override void OnUpgrade() => DynamicVars["DivineProtectionPlatingPower"].UpgradeValueBy(1m);
}
