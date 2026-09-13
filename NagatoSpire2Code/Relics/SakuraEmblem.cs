using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using NagatoSpire2.NagatoSpire2Code.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NagatoSpire2.NagatoSpire2Code.Relics;

[RegisterCharacterStarterRelic(typeof(NagatoCharacter))]
public sealed class SakuraEmblem : NagatoRelicModel
{
	public override RelicRarity Rarity => RelicRarity.Starter;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(6M)];

	public override async Task AfterCombatVictory(CombatRoom _)
	{
		if (Owner.Creature.IsDead)
		{
			return;
		}

		Flash();
		await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);
	}
}
