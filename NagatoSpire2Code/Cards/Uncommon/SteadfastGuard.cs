using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class SteadfastGuard() : NagatoCardModel(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("ExtraBlock", 2)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await PowerCmd.Apply<SteadfastGuardPower>(choiceContext, Owner.Creature,
			DynamicVars["ExtraBlock"].BaseValue, Owner.Creature, this);

	protected override void OnUpgrade() => DynamicVars["ExtraBlock"].UpgradeValueBy(1m);
}
