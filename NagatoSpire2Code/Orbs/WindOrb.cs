using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

public sealed class WindOrb : NagatoOrbModel
{
	public override Color DarkenedColor => new("#477c6b");
	public override decimal PassiveVal => 2m;
	public override decimal EvokeVal => CardPile.MaxCardsInHand;
	public override ModOrbValueDisplayMode ValueDisplayMode => ModOrbValueDisplayMode.SinglePassive;

	protected override bool HasPlayPassive => true;

	public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
	{
		ActivatePassive();
		await CardPileCmd.Draw(choiceContext, PassiveVal, Owner);
	}

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		ActivateEvoke([Owner.Creature]);
		await CardPileCmd.Draw(choiceContext, CardPile.MaxCardsInHand - Owner.PlayerCombatState!.Hand.Cards.Count, Owner);
		return [Owner.Creature];
	}
}
