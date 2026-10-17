using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

public sealed class ThunderOrb : NagatoOrbModel
{
	public override Color DarkenedColor => new("#555e8f");
	public override decimal PassiveVal => FocusValue(3m);
	public override decimal EvokeVal => FocusValue(3m);
	public override ModOrbValueDisplayMode ValueDisplayMode => ModOrbValueDisplayMode.SingleEvoke;
	public override bool IsFocusAffected => true;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromAffliction<Sealed>();

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		var targets = CombatState.HittableEnemies.ToArray();
		ActivateEvoke(targets);
		int hits = (int)PassiveVal;
		for (int i = 0; i < hits; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
				break;
			await CreatureCmd.Damage(choiceContext, targets.Where(creature => creature.IsHittable), EvokeVal,
				ValueProp.Unpowered, Owner.Creature);
		}
		return targets;
	}
}
