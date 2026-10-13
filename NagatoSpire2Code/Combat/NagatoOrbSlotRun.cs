using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using NagatoSpire2.NagatoSpire2Code.Characters;
using NagatoSpire2.NagatoSpire2Code.Configs;
using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public sealed class NagatoOrbSlotRunData
{
	public bool OtherCharacters { get; set; }
}

public static class NagatoOrbSlotRun
{
	public static RunSavedData<NagatoOrbSlotRunData> Data { get; private set; } = null!;

	public static void Register()
	{
		Data = RitsuLibFramework.GetRunSavedDataStore(MainFile.ModId).Register(
			"orb-card-slots", static () => new NagatoOrbSlotRunData());
		RitsuLibFramework.SubscribeLifecycle<RunSavedDataLobbyStagingEvent>(evt =>
		{
			if (evt.IsHost && evt.Reason == RunSavedDataLobbyStagingReason.Committing)
				Data.Lobby.Set(evt.Lobby, new() { OtherCharacters = NagatoConfigPage.OtherCharactersOrbSlotsBinding.Read() });
		});
		RitsuLibFramework.SubscribeLifecycle<RunSavedDataPreparingEvent>(evt =>
		{
			if (!evt.IsMultiplayer && !Data.TryGet(evt.RunState, out _))
				Data.Set(evt.RunState, new() { OtherCharacters = NagatoConfigPage.OtherCharactersOrbSlotsBinding.Read() });
		});
	}

	public static bool AppliesTo(Player player) => player.Character is NagatoCharacter ||
		player.RunState is RunState run && Data != null && Data.TryGet(run, out var data) && data.OtherCharacters;
}
