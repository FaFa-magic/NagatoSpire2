using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class FleetRegroup() : NagatoCardModel(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<FleetRegroupPower>()];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await PowerCmd.Apply<FleetRegroupPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
