using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class EmergencyPlating() : NagatoCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self), IModelCapabilitySource
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<TemporaryPlatingPower>(5m)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	public void BuildDefaultCapabilities(ModelCapabilityList capabilities) =>
		capabilities.Add<CardOrbCapability>().SetOrbs(ModelDb.Orb<MountainOrb>().ToMutable());

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<TemporaryPlatingPower>(choiceContext, Owner.Creature,
			DynamicVars["TemporaryPlatingPower"].BaseValue, Owner.Creature, this);
		await NagatoPlatingCmd.Trigger(choiceContext, Owner);
	}

	protected override void OnUpgrade() => DynamicVars["TemporaryPlatingPower"].UpgradeValueBy(2m);
}
