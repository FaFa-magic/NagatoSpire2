using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class MoveAsThunder() : NagatoCardModel(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies), IModelCapabilitySource
{
	public void BuildDefaultCapabilities(ModelCapabilityList capabilities) =>
		capabilities.Add<CardOrbCapability>().SetOrbs(ModelDb.Orb<ThunderOrb>().ToMutable(), ModelDb.Orb<ThunderOrb>().ToMutable());

	public override Task BeforeCombatStart() => Pile?.IsCombatPile == true
		? CardPileCmd.Add(this, PileType.Draw, CardPilePosition.Bottom, skipVisuals: true) : Task.CompletedTask;

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

	protected override void OnUpgrade() => NagatoCardOrbs.BindOnUpgrade(this, ModelDb.Orb<ThunderOrb>().ToMutable());
}
