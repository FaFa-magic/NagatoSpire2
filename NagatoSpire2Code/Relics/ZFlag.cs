using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Relics;

public sealed class ZFlag : NagatoRelicModel, INagatoSalvoListener
{
	private bool _triggered;
	public override RelicRarity Rarity => RelicRarity.Uncommon;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<StrengthPower>(1m), new PowerVar<FocusPower>(1m)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[NagatoHoverTips.Salvo, HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromPower<FocusPower>()];

	public override Task BeforeCombatStart()
	{
		_triggered = false;
		Status = RelicStatus.Normal;
		return Task.CompletedTask;
	}

	public async Task AfterNagatoSalvo(PlayerChoiceContext choiceContext)
	{
		if (_triggered || Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		_triggered = true;
		Status = RelicStatus.Disabled;
		Flash();
		await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, null);
		await PowerCmd.Apply<FocusPower>(choiceContext, Owner.Creature, DynamicVars["FocusPower"].BaseValue, Owner.Creature, null);
	}
}
