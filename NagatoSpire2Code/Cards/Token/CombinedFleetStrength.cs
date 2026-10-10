using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class CombinedFleetStrength : NagatoFleetChoiceOption
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(4m)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

	public override async Task ApplyTo(PlayerChoiceContext choiceContext, Player player, CardModel? source) =>
		await PowerCmd.Apply<StrengthPower>(choiceContext, player.Creature,
			DynamicVars["StrengthPower"].BaseValue, source?.Owner.Creature ?? player.Creature, source);
}
