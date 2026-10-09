using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Commands;

namespace NagatoSpire2.NagatoSpire2Code.Cards;

public abstract class NagatoShellChoiceOption<TOrb> : NagatoChoiceOption where TOrb : OrbModel
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.Static(StaticHoverTip.Evoke),
		HoverTipFactory.FromOrb<TOrb>()
	];

	public override Task OnChosen(PlayerChoiceContext choiceContext)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return Task.CompletedTask;

		return NagatoOrbCmd.Evoke(choiceContext, Owner, state.OrbQueue.Orbs.OfType<TOrb>().ToArray());
	}

	protected override void OnUpgrade() { }
}
