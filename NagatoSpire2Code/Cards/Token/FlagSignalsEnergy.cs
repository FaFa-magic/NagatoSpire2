using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class FlagSignalsEnergy : NagatoFleetChoiceOption
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [EnergyHoverTip];

	public override Task ApplyTo(PlayerChoiceContext choiceContext, Player player, CardModel? source) =>
		PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, player);

	protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1m);
}
