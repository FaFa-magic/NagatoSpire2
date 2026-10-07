using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class DivineDecreeEnergy : NagatoChoiceOption
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [EnergyHoverTip];

	public override Task OnChosen(PlayerChoiceContext choiceContext) => PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);

	protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1m);
}
