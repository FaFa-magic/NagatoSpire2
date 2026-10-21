using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Ancient;

public sealed class NagatosEdict() : NagatoCardModel(41, CardType.Power, CardRarity.Ancient, TargetType.Self), IModelCapabilitySource
{
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. ModelDb.Orb<WindOrb>().AssetPaths];

	public void BuildDefaultCapabilities(ModelCapabilityList capabilities)
	{
		capabilities.Add<PermanentSealedCapability>();
		capabilities.Add<CardOrbCapability>();
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await PowerCmd.Apply<NagatosEdictPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

	protected override void OnUpgrade() => NagatoCardOrbs.BindOnUpgrade(this, ModelDb.Orb<WindOrb>().ToMutable());
}
