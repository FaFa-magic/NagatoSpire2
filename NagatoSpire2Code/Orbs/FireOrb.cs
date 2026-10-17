using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

public sealed class FireOrb : NagatoOrbModel
{
	public override Color DarkenedColor => new("#8c453b");
	public override decimal PassiveVal => FocusValue(1m);
	public override decimal EvokeVal => FocusValue(2m);
	public override ModOrbValueDisplayMode ValueDisplayMode => ModOrbValueDisplayMode.Contextual;
	public override bool IsFocusAffected => true;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

	public override Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result,
		ValueProp props, Creature target, CardModel? cardSource) =>
		cardSource != null && cardSource.Owner == Owner && result.TotalDamage > 0 && NagatoCardOrbs.Get(cardSource).Contains(this)
			? OrbCmd.Passive(choiceContext, this, null, true) : Task.CompletedTask;

	public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
	{
		ActivatePassive();
		await PowerCmd.Apply<FireStrengthPower>(choiceContext, Owner.Creature, PassiveVal, Owner.Creature, null);
	}

	public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
	{
		ActivateEvoke([Owner.Creature]);
		await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, EvokeVal, Owner.Creature, null);
		return [Owner.Creature];
	}
}
