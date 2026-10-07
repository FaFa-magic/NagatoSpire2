using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class EmergencyPlating() : NagatoCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<EmergencyPlatingPower>(5m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<EmergencyPlatingPower>()];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<EmergencyPlatingPower>(choiceContext, Owner.Creature,
			DynamicVars["EmergencyPlatingPower"].BaseValue, Owner.Creature, this);
		await NagatoPlatingCmd.Trigger(choiceContext, Owner);
	}

	protected override void OnUpgrade() => DynamicVars["EmergencyPlatingPower"].UpgradeValueBy(2m);
}
