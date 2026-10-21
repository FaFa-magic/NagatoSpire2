using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class CedarPalisade() : NagatoCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self), IModelCapabilitySource
{
	public override bool GainsBlock => true;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8m, ValueProp.Move)];

	public void BuildDefaultCapabilities(ModelCapabilityList capabilities) =>
		capabilities.Add<CardOrbCapability>().SetOrbs(ModelDb.Orb<ForestOrb>().ToMutable());

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

	protected override void OnUpgrade()
	{
		DynamicVars.Block.UpgradeValueBy(2m);
		NagatoCardOrbs.BindOnUpgrade(this, ModelDb.Orb<ForestOrb>().ToMutable());
	}
}
