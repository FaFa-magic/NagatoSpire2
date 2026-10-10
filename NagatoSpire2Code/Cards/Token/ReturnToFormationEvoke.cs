using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class ReturnToFormationEvoke : NagatoChoiceOption
{
	public override OrbEvokeType OrbEvokeType => OrbEvokeType.Front;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(1)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

	public override async Task OnChosen(PlayerChoiceContext choiceContext)
	{
		if (Owner.PlayerCombatState is not { } state)
			return;

		for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead ||
				Owner.PlayerCombatState != state || state.OrbQueue.Orbs.Count == 0)
				break;

			await OrbCmd.EvokeNext(choiceContext, Owner);
		}
	}

	protected override void OnUpgrade() => DynamicVars.Repeat.UpgradeValueBy(1m);
}
