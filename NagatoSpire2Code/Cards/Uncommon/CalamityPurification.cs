using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class CalamityPurification() : NagatoCardModel(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromAffliction<Sealed>();
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, NSealedCardVfx.ScenePath];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await PowerCmd.Apply<CalamityPurificationPower>(choiceContext, Owner.Creature,
			DynamicVars.Cards.BaseValue, Owner.Creature, this);

	protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
