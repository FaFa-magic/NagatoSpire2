using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class NavyHoliday() : NagatoCardModel(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(1)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<NavyHolidayPower>(choiceContext, Owner.Creature, DynamicVars.Repeat.BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade() => DynamicVars.Repeat.UpgradeValueBy(1m);
}
