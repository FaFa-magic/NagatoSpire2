using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class OperationVictory() : NagatoCardModel(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. NagatoOrbPool.AssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await PowerCmd.Apply<OperationVictoryPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
