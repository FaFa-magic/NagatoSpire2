using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Utils;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public static class NagatoOrbHistory
{
	private static readonly AttachedState<CombatHistory, Dictionary<ulong, int>> Counts = new(() => []);
	private static readonly AttachedState<CombatHistory, Dictionary<(ulong, Type), int>> TypeCounts = new(() => []);

	public static int GetEvokeCount(Player player)
	{
		return Counts.TryGetValue(CombatManager.Instance.History, out var counts)
			? counts.GetValueOrDefault(player.NetId)
			: 0;
	}

	public static void Record(ICombatState combatState, OrbModel orb)
	{
		if (!combatState.IsLiveCombat())
			return;

		var counts = Counts.GetOrCreate(CombatManager.Instance.History);
		ulong id = orb.Owner.NetId;
		counts[id] = counts.GetValueOrDefault(id) + 1;
		var types = TypeCounts.GetOrCreate(CombatManager.Instance.History);
		var key = (id, orb.GetType());
		types[key] = types.GetValueOrDefault(key) + 1;
	}

	public static int GetEvokeCount<TOrb>(Player player) where TOrb : OrbModel =>
		TypeCounts.TryGetValue(CombatManager.Instance.History, out var counts)
			? counts.GetValueOrDefault((player.NetId, typeof(TOrb)))
			: 0;

	public static void Clear(CombatHistory history)
	{
		Counts.Remove(history);
		TypeCounts.Remove(history);
	}
}
