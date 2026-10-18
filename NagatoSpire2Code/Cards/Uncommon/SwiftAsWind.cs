using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class SwiftAsWind() : NagatoCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self), IModelCapabilitySource
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

	public void BuildDefaultCapabilities(ModelCapabilityList capabilities) =>
		capabilities.Add<CardOrbCapability>().SetOrbs(ModelDb.Orb<WindOrb>().ToMutable(), ModelDb.Orb<WindOrb>().ToMutable());

	public override Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw) =>
		card == this && Owner.PlayerCombatState != null && !Owner.Creature.IsDead && !CombatManager.Instance.IsOverOrEnding
			? NagatoCardOrbs.Bind(choiceContext, this, ModelDb.Orb<WindOrb>().ToMutable()) : Task.CompletedTask;

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

	protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Retain);
}
