using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class DivineWillPlating : NagatoChoiceOption
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DivineWillPlatingPower>(3m)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	public override async Task OnChosen(PlayerChoiceContext choiceContext) =>
		await PowerCmd.Apply<DivineWillPlatingPower>(choiceContext, Owner.Creature,
			DynamicVars["DivineWillPlatingPower"].BaseValue, Owner.Creature, this);

	protected override void OnUpgrade() => DynamicVars["DivineWillPlatingPower"].UpgradeValueBy(1m);
}
