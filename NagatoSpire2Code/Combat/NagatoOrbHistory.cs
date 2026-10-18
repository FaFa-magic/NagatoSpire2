using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public static class NagatoOrbHistory
{
	private static PlayerRunSavedData<NagatoOrbHistoryData> Recent = null!;

	public static void Register() => Recent = RitsuLibFramework.GetRunSavedDataStore(MainFile.ModId)
		.RegisterPerPlayer("recent-evoked-orbs", static () => new NagatoOrbHistoryData());

	public static IReadOnlyList<OrbModel> GetRecentOrbs(Player player) => Recent.Get(player).Orbs
		.Select(id => ModelDb.GetById<OrbModel>(ModelId.Deserialize(id)).ToMutable()).ToArray();

	public static void Record(ICombatState combatState, OrbModel orb)
	{
		if (!combatState.IsLiveCombat())
			return;

		Recent.Modify(orb.Owner, data =>
		{
			data.Orbs.Add(orb.Id.ToString());
			if (data.Orbs.Count > 2)
				data.Orbs.RemoveAt(0);
		});
	}
}

public sealed class NagatoOrbHistoryData
{
	public List<string> Orbs { get; set; } = [];
}
