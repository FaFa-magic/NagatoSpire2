using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace NagatoSpire2.NagatoSpire2Code.Relics;

public sealed class SacredFurnace : NagatoRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Ancient;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];

	protected override bool IncludeEnergyHoverTip => true;

	public override decimal ModifyMaxEnergy(Player player, decimal amount) =>
		player == Owner ? amount + DynamicVars.Energy.IntValue : amount;

	public override decimal ModifyEnergyGain(Player player, decimal amount) =>
		player == Owner && player.Creature.CombatState?.CurrentSide == player.Creature.Side ? 0m : amount;

	public override Task AfterModifyingEnergyGain()
	{
		Flash();
		return Task.CompletedTask;
	}
}
