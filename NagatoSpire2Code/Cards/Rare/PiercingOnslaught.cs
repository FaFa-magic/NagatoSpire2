using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class PiercingOnslaught() : NagatoCardModel(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy), IModelCapabilitySource
{
	public void BuildDefaultCapabilities(ModelCapabilityList capabilities) =>
		capabilities.Add<CardOrbCapability>().SetOrbs(
			ModelDb.Orb<ForestOrb>().ToMutable(), ModelDb.Orb<FireOrb>().ToMutable(), ModelDb.Orb<ThunderOrb>().ToMutable());

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

	protected override void OnUpgrade() { }
}
