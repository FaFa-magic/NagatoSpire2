using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class CombinedFleetEnergy : NagatoFleetChoiceOption
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [EnergyHoverTip];

	public override async Task ApplyTo(PlayerChoiceContext choiceContext, Player player, CardModel? source) =>
		await PowerCmd.Apply<CombinedFleetEnergyPower>(choiceContext, player.Creature,
			DynamicVars.Energy.BaseValue, source?.Owner.Creature ?? player.Creature, source);
}
