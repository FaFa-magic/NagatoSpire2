using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using NagatoSpire2.NagatoSpire2Code.Characters;
using NagatoSpire2.NagatoSpire2Code.Configs;
using STS2RitsuLib;
using STS2RitsuLib.RunData;

namespace NagatoSpire2.NagatoSpire2Code.Acts;

public sealed class NagatoFourthActRunData
{
    public bool Enabled { get; set; }
}

public static class InkStainedSteelSakuraRun
{
    private static readonly Action<RunState, IReadOnlyList<ActModel>> SetActs =
        AccessTools.PropertySetter(typeof(RunState), nameof(RunState.Acts))
            .CreateDelegate<Action<RunState, IReadOnlyList<ActModel>>>();

    public static RunSavedData<NagatoFourthActRunData> Data { get; private set; } = null!;

    public static void Register()
    {
        Data = RitsuLibFramework.GetRunSavedDataStore(MainFile.ModId).Register(
            "fourth-act", static () => new NagatoFourthActRunData());
        RitsuLibFramework.SubscribeLifecycle<RunSavedDataLobbyStagingEvent>(evt =>
        {
            if (evt.IsHost && evt.Reason == RunSavedDataLobbyStagingReason.Committing)
                Data.Lobby.Set(evt.Lobby, new() { Enabled = NagatoConfigPage.FourthActBinding.Read() });
        });
        RitsuLibFramework.SubscribeLifecycle<RunSavedDataPreparingEvent>(evt =>
        {
            if (!evt.IsMultiplayer && !Data.TryGet(evt.RunState, out _))
                Data.Set(evt.RunState, new() { Enabled = NagatoConfigPage.FourthActBinding.Read() });
        });
    }

    public static void EnsureAct(RunState? runState)
    {
        if (runState == null || runState.Acts.Count != 3 || runState.CurrentActIndex > 2
            || runState.CurrentRoom?.IsVictoryRoom == true
            || !runState.Players.Any(player => player.Character is NagatoCharacter)
            || !Data.TryGet(runState, out var data) || !data.Enabled)
            return;

        ActModel thirdAct = runState.Acts[2];
        if (thirdAct.SecondBossEncounter == null)
        {
            EncounterModel[] bosses = thirdAct.AllBossEncounters
                .Where(encounter => encounter.Id != thirdAct.BossEncounter.Id).ToArray();
            if (bosses.Length == 0)
                return;
            thirdAct.SetSecondBossEncounter(runState.Rng.UpFront.NextItem(bosses));
        }
        SetActs(runState, [.. runState.Acts, InkStainedSteelSakura.CreateForRun()]);
    }

    public static bool HasFourthAct(IRunState runState) =>
        runState.Acts.Count == 4 && runState.Acts[3] is InkStainedSteelSakura;
}
