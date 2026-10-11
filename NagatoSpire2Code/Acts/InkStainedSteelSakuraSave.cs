using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace NagatoSpire2.NagatoSpire2Code.Acts;

public static class InkStainedSteelSakuraSave
{
    public static void NormalizeRun(SerializableRun save)
    {
        foreach (SerializableActModel act in save.Acts)
            NormalizeRoomLists(act);
        if (save.CurrentActIndex >= 0 && save.CurrentActIndex < save.Acts.Count
            && save.Acts[save.CurrentActIndex].Id == ModelDb.GetId<InkStainedSteelSakura>()
            && save.PreFinishedRoom is { } room && room.EncounterId == ModelDb.GetId<QueenBoss>())
        {
            room.EncounterId = InkStainedSteelSakura.Boss.Id;
            room.EncounterState.Clear();
        }
    }

    public static void NormalizeRoomLists(SerializableActModel save)
    {
        if (save.Id != ModelDb.GetId<InkStainedSteelSakura>() || save.SerializableRooms is not { } rooms)
            return;

        rooms.EventIds ??= [];
        rooms.NormalEncounterIds ??= [];
        rooms.EliteEncounterIds ??= [];
        if (rooms.BossId == null || rooms.BossId == ModelDb.GetId<QueenBoss>())
            rooms.BossId = InkStainedSteelSakura.Boss.Id;
        if (rooms.EliteEncounterIds.Count == 0
            && (save.SavedMap == null || save.SavedMap.Points.Any(point => point.PointType == MapPointType.Elite)))
            rooms.EliteEncounterIds.AddRange(InkStainedSteelSakura.InitialEliteEncounters.Select(encounter => encounter.Id));
    }
}

public static class InkStainedSteelSakuraRestSiteContext
{
    [ThreadStatic]
    private static Stack<IRunState>? _runs;

    public static bool Enter(IRunState runState)
    {
        if (runState.Act is not InkStainedSteelSakura)
            return false;
        (_runs ??= new()).Push(runState);
        return true;
    }

    public static bool IsActive(IRunState runState) => _runs?.Contains(runState) == true;

    public static void Exit(bool entered)
    {
        if (entered)
            _runs!.Pop();
    }
}
