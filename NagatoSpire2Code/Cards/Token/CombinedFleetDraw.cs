using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class CombinedFleetDraw : NagatoFleetChoiceOption
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<CombinedFleetDrawPower>()];

	public override async Task ApplyTo(PlayerChoiceContext choiceContext, Player player, CardModel? source) =>
		await PowerCmd.Apply<CombinedFleetDrawPower>(choiceContext, player.Creature,
			DynamicVars.Cards.BaseValue, source?.Owner.Creature ?? player.Creature, source);
}
