using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Commands;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

public sealed class ShadowOrb : NagatoOrbModel
{
	public override Color DarkenedColor => new("#5c4876");
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

	public override Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		cardPlay.Player == Owner && NagatoCardOrbs.Get(cardPlay.Card).Contains(this)
			? OrbCmd.Passive(choiceContext, this, null, true) : Task.CompletedTask;

	public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
	{
		if (HostCard is not { HasBeenRemovedFromState: false } card || Owner.Creature.CombatState is not { } combat)
			return;
		ActivatePassive();
		var putBack = combat.CreateCard<ShadowReturn>(Owner);
		var exhaust = combat.CreateCard<ShadowExhaust>(Owner);
		putBack.SourceCard = card;
		exhaust.SourceCard = card;
		await NagatoChoiceCmd.Choose(choiceContext, Owner, [putBack, exhaust]);
	}

	public override Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		ActivateEvoke([Owner.Creature]);
		HostCard?.GetOrCreateCapability<CardOrbCapability>().QueueShadowPlay();
		return Task.FromResult<IEnumerable<Creature>>([Owner.Creature]);
	}
}
