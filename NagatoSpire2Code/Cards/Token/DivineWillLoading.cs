using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class DivineWillLoading : NagatoChoiceOption
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[NagatoHoverTips.Loading, HoverTipFactory.FromCard<Shell>(IsUpgraded)];

	public override async Task OnChosen(PlayerChoiceContext choiceContext)
	{
		int before = Owner.Creature.GetPowerAmount<DivineWillPower>();
		var power = await PowerCmd.Apply<DivineWillPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
		if (IsUpgraded && power != null)
			power.UpgradedShells += Math.Max(power.Amount - before, 0);
	}

	protected override void OnUpgrade() { }
}
