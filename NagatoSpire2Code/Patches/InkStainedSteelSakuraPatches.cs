using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.Saves;
using NagatoSpire2.NagatoSpire2Code.Acts;
using NagatoSpire2.NagatoSpire2Code.Encounters;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class InkStainedSteelSakuraRunPatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_run";
    public static string Description => "Append the saved fourth-act route after native third-act room generation";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() => [new(typeof(RunManager), nameof(RunManager.GenerateRooms))];

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(RunManager __instance) => InkStainedSteelSakuraRun.EnsureAct(__instance.DebugOnlyGetState());
}

public sealed class InkStainedSteelSakuraMapPatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_map";
    public static string Description => "Create the fixed seven-node fourth-act route";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(ActModel), nameof(ActModel.CreateMap), [typeof(RunState), typeof(bool)])];

    [HarmonyPrefix]
    public static bool Prefix(ActModel __instance, ref ActMap __result)
    {
        if (__instance is not InkStainedSteelSakura)
            return true;
        __result = new InkStainedSteelSakuraMap();
        return false;
    }
}

public sealed class InkStainedSteelSakuraFloorCountPatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_floor_count";
    public static string Description => "Keep the fourth act at seven floors in singleplayer and multiplayer";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(ActModel), nameof(ActModel.GetNumberOfRooms), [typeof(bool)]),
        new(typeof(ActModel), nameof(ActModel.GetNumberOfFloors), [typeof(bool)])
    ];

    [HarmonyPrefix]
    public static bool Prefix(ActModel __instance, MethodBase __originalMethod, ref int __result)
    {
        if (__instance is not InkStainedSteelSakura)
            return true;
        __result = __originalMethod.Name == nameof(ActModel.GetNumberOfFloors)
            ? InkStainedSteelSakuraMap.RoomCount : InkStainedSteelSakuraMap.RoomCount - 1;
        return false;
    }
}

public sealed class InkStainedSteelSakuraMapRestorePatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_map_restore";
    public static string Description => "Restore fixed node flags on loaded fourth-act maps";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(Hook), nameof(Hook.ModifyGeneratedMapLate), [typeof(IRunState), typeof(ActMap), typeof(int)])];

    [HarmonyPrefix]
    public static void Prefix(IRunState runState, ActMap map, int actIndex)
    {
        if (actIndex < 0 || actIndex >= runState.Acts.Count || runState.Acts[actIndex] is not InkStainedSteelSakura)
            return;
        map.StartingMapPoint.CanBeModified = false;
        map.BossMapPoint.CanBeModified = false;
        foreach (MapPoint point in map.GetAllMapPoints())
            point.CanBeModified = false;
    }
}

public sealed class InkStainedSteelSakuraBossSavePatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_boss_save";
    public static string Description => "Migrate fourth-act placeholder encounters before native load and host canonicalization";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(RunState), nameof(RunState.FromSerializable), [typeof(SerializableRun)]),
        new(typeof(RunManager), nameof(RunManager.CanonicalizeSave), [typeof(SerializableRun), typeof(ulong)])
    ];

    [HarmonyPrefix]
    public static void Prefix(SerializableRun save) => InkStainedSteelSakuraSave.NormalizeRun(save);
}

public sealed class InkStainedSteelSakuraMultiplayerScalingPatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_multiplayer_scaling";
    public static string Description => "Use native third-act multiplayer scaling for Nagato's fourth act";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(MultiplayerScalingModel), nameof(MultiplayerScalingModel.GetMultiplayerScaling), [typeof(EncounterModel), typeof(int)])];

    [HarmonyPrefix]
    public static void Prefix(EncounterModel? encounter, ref int actIndex)
    {
        if (actIndex == 3 && (encounter is NagatoBossEncounter
            || RunManager.Instance.DebugOnlyGetState()?.Act is InkStainedSteelSakura))
            actIndex = 2;
    }
}

public sealed class InkStainedSteelSakuraActLoadPatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_load";
    public static string Description => "Restore omitted fourth-act room collections before native save loading";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(ActModel), nameof(ActModel.FromSave), [typeof(SerializableActModel)])];

    [HarmonyPrefix]
    public static void Prefix(SerializableActModel save) => InkStainedSteelSakuraSave.NormalizeRoomLists(save);
}

public sealed class InkStainedSteelSakuraActSerializePatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_serialize";
    public static string Description => "Normalize fourth-act room collections before native network serialization";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(SerializableActModel), nameof(SerializableActModel.Serialize), [typeof(PacketWriter)])];

    [HarmonyPrefix]
    public static void Prefix(SerializableActModel __instance) => InkStainedSteelSakuraSave.NormalizeRoomLists(__instance);
}

public sealed class InkStainedSteelSakuraTreasureRoomPatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_treasure";
    public static string Description => "Reuse the native Glory treasure room in the fourth act";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(RunManager), "CreateRoom", [typeof(RoomType), typeof(MapPointType), typeof(AbstractModel)])];

    [HarmonyPrefix]
    public static bool Prefix(RunManager __instance, RoomType roomType, ref AbstractRoom __result)
    {
        if (__instance.DebugOnlyGetState()?.Act is not InkStainedSteelSakura || roomType != RoomType.Treasure)
            return true;
        __result = new TreasureRoom(2);
        return false;
    }
}

public sealed class InkStainedSteelSakuraRestSiteActIndexPatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_rest_site_index";
    public static string Description => "Use native warm-lit rest animations during fourth-act rest initialization";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(RunState), nameof(RunState.CurrentActIndex), null, MethodType.Getter)];

    [HarmonyPostfix]
    public static void Postfix(RunState __instance, ref int __result)
    {
        if (__result == 3 && InkStainedSteelSakuraRestSiteContext.IsActive(__instance))
            __result = 0;
    }
}

public sealed class InkStainedSteelSakuraRestSiteReadyPatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_rest_site_ready";
    public static string Description => "Keep native rest characters compatible with the fourth act in mixed parties";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() => [new(typeof(NRestSiteCharacter), nameof(NRestSiteCharacter._Ready))];

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void Prefix(NRestSiteCharacter __instance, out bool __state) =>
        __state = InkStainedSteelSakuraRestSiteContext.Enter(__instance.Player.RunState);

    [HarmonyFinalizer]
    public static void Finalizer(bool __state) => InkStainedSteelSakuraRestSiteContext.Exit(__state);
}

public sealed class InkStainedSteelSakuraRestSiteLayoutPatch : IPatchMethod
{
    private static readonly AccessTools.FieldRef<NRestSiteRoom, IRunState> RunState =
        AccessTools.FieldRefAccess<NRestSiteRoom, IRunState>("_runState");
    private static readonly Vector2[] Positions =
    [
        new(570f, 735f),
        new(1310f, 735f),
        new(680f, 590f),
        new(1200f, 590f)
    ];

    public static string PatchId => "nagato_fourth_act_rest_site_layout";
    public static string Description => "Place native multiplayer resting characters around the indoor hearth";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() => [new(typeof(NRestSiteRoom), nameof(NRestSiteRoom._Ready))];

    [HarmonyPostfix]
    public static void Postfix(NRestSiteRoom __instance)
    {
        if (RunState(__instance)?.Act is not InkStainedSteelSakura)
            return;
        Control container = __instance.GetNode<Control>("BgContainer");
        for (int index = 0; index < Positions.Length; index++)
            container.GetNode<Control>($"Character_{index + 1}").Position = Positions[index];
    }
}

public sealed class InkStainedSteelSakuraBossRewardsPatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_third_act_rewards";
    public static string Description => "Keep native boss continuation screens without third-act rewards on the fourth-act route";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(RewardsCmd), nameof(RewardsCmd.GenerateForRoomEnd), [typeof(Player), typeof(AbstractRoom)])];

    [HarmonyPrefix]
    public static bool Prefix(Player player, AbstractRoom room, ref Task<RewardsSet> __result)
    {
        if (room is not CombatRoom { RoomType: RoomType.Boss }
            || player.RunState.CurrentActIndex != 2
            || !InkStainedSteelSakuraRun.HasFourthAct(player.RunState))
            return true;
        __result = GenerateEmptyRewards(player, room);
        return false;
    }

    private static async Task<RewardsSet> GenerateEmptyRewards(Player player, AbstractRoom room)
    {
        RewardsSet rewards = new RewardsSet(player).EmptyForRoom(room);
        await rewards.GenerateWithoutOffering();
        rewards.Rewards.Clear();
        return rewards;
    }
}
