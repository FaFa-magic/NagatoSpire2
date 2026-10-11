using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.sts2.Core.Nodes.TopBar;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using NagatoSpire2.NagatoSpire2Code.Acts;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public static class InkStainedSteelSakuraBossPresentation
{
    public static void ApplyIcon(NClickableControl owner)
    {
        Texture2D texture = ResourceLoader.Load<Texture2D>(InkStainedSteelSakura.BossHudIconPath);
        owner.GetNode<TextureRect>("Icon").Texture = texture;
        owner.GetNode<TextureRect>("Icon/Outline").Texture = texture;
    }
}

public sealed class InkStainedSteelSakuraTopBarBossPatch : IPatchMethod
{
    private static readonly AccessTools.FieldRef<NTopBarBossIcon, IRunState> RunState =
        AccessTools.FieldRefAccess<NTopBarBossIcon, IRunState>("_runState");

    public static string PatchId => "nagato_fourth_act_top_bar_boss";
    public static string Description => "Display Nagato's fourth-act boss portrait and localized hover tip";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(NTopBarBossIcon), nameof(NTopBarBossIcon.RefreshBossIcon)),
        new(typeof(NTopBarBossIcon), "OnFocus")
    ];

    [HarmonyPrefix]
    public static bool Prefix(NTopBarBossIcon __instance, MethodBase __originalMethod)
    {
        if (RunState(__instance)?.Act is not InkStainedSteelSakura)
            return true;

        if (__originalMethod.Name == nameof(NTopBarBossIcon.RefreshBossIcon))
        {
            InkStainedSteelSakuraBossPresentation.ApplyIcon(__instance);
            __instance.GetNodeOrNull<Control>("Icon/SecondBossIcon")?.Hide();
        }
        else
        {
            LocString name = new("acts", InkStainedSteelSakura.BossTitleKey);
            LocString title = new("static_hover_tips", "BOSS.title");
            LocString description = new("static_hover_tips", "BOSS.description");
            title.Add("BossName", name);
            description.Add("BossName", name);
            HoverTip tip = new(title, description);
            NHoverTipSet.CreateAndShow(__instance, tip)?.SetGlobalPosition(
                __instance.GetNode<TextureRect>("Icon").GlobalPosition + new Vector2(0f, __instance.Size.Y + 20f));
        }
        return false;
    }
}

public sealed class InkStainedSteelSakuraTopBarRoomPatch : IPatchMethod
{
    private static readonly AccessTools.FieldRef<NTopBarRoomIcon, IRunState> RunState =
        AccessTools.FieldRefAccess<NTopBarRoomIcon, IRunState>("_runState");

    public static string PatchId => "nagato_fourth_act_top_bar_room";
    public static string Description => "Keep the fourth-act boss portrait consistent on the current-room indicator";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() => [new(typeof(NTopBarRoomIcon), "UpdateIcon")];

    [HarmonyPostfix]
    public static void Postfix(NTopBarRoomIcon __instance)
    {
        IRunState run = RunState(__instance);
        if (run?.Act is InkStainedSteelSakura && run.CurrentMapPoint?.PointType == MapPointType.Boss
            && run.BaseRoom is { RoomType: RoomType.Boss, IsVictoryRoom: false })
            InkStainedSteelSakuraBossPresentation.ApplyIcon(__instance);
    }
}

public sealed class InkStainedSteelSakuraMapMarkerPatch : IPatchMethod
{
    private static readonly AccessTools.FieldRef<NMapMarker, Vector2> PositionOffset =
        AccessTools.FieldRefAccess<NMapMarker, Vector2>("_posOffset");

    public static string PatchId => "nagato_fourth_act_map_marker";
    public static string Description => "Position the native marker above transformed boss nodes on the fourth-act route";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(NMapMarker), nameof(NMapMarker.SetMapPoint), [typeof(NMapPoint)])];

    [HarmonyPrefix]
    public static void Prefix(NMapMarker __instance, NMapPoint node, out Vector2? __state)
    {
        __state = null;
        if (node is not NBossMapPoint || __instance.GetParent() is not CanvasItem parent
            || RunManager.Instance.DebugOnlyGetState() is not { CurrentActIndex: 2 or 3 } runState
            || !InkStainedSteelSakuraRun.HasFourthAct(runState))
            return;

        Vector2 topCenter = new(node.Size.X / 2f, 0f);
        Vector2 transformedCenter = parent.GetGlobalTransform().AffineInverse() * (node.GetGlobalTransform() * topCenter);
        __state = PositionOffset(__instance);
        PositionOffset(__instance) += transformedCenter - node.Position - topCenter;
    }

    [HarmonyFinalizer]
    public static void Finalizer(NMapMarker __instance, Vector2? __state)
    {
        if (__state.HasValue)
            PositionOffset(__instance) = __state.Value;
    }
}

public sealed class InkStainedSteelSakuraMapTexturePatch : IPatchMethod
{
    public static string PatchId => "nagato_fourth_act_map_texture";
    public static string Description => "Display contiguous scroll regions using native map background panels";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(ActModel), nameof(ActModel.MapTopBg), null, MethodType.Getter),
        new(typeof(ActModel), nameof(ActModel.MapMidBg), null, MethodType.Getter),
        new(typeof(ActModel), nameof(ActModel.MapBotBg), null, MethodType.Getter)
    ];

    [HarmonyPrefix]
    public static bool Prefix(ActModel __instance, MethodBase __originalMethod, ref Texture2D __result)
    {
        if (__instance is not InkStainedSteelSakura)
            return true;
        string region = __originalMethod.Name switch
        {
            "get_MapTopBg" => "top",
            "get_MapMidBg" => "middle",
            _ => "bottom"
        };
        __result = ResourceLoader.Load<AtlasTexture>(InkStainedSteelSakura.MapAssetDirectory + $"map_{region}.tres");
        return false;
    }
}

public sealed class InkStainedSteelSakuraBossIconPatch : IPatchMethod
{
    private static readonly AccessTools.FieldRef<NBossMapPoint, ActModel> Act =
        AccessTools.FieldRefAccess<NBossMapPoint, ActModel>("_act");
    private static readonly AccessTools.FieldRef<NBossMapPoint, bool> UsesSpine =
        AccessTools.FieldRefAccess<NBossMapPoint, bool>("_usesSpine");
    private static readonly AccessTools.FieldRef<NBossMapPoint, TextureRect> Image =
        AccessTools.FieldRefAccess<NBossMapPoint, TextureRect>("_placeholderImage");
    private static readonly AccessTools.FieldRef<NBossMapPoint, TextureRect> Outline =
        AccessTools.FieldRefAccess<NBossMapPoint, TextureRect>("_placeholderOutline");

    public static string PatchId => "nagato_fourth_act_boss_icon";
    public static string Description => "Use Nagato's ink portrait only on the custom fourth-act boss node";
    public static bool IsCritical => true;
    public static ModPatchTarget[] GetTargets() => [new(typeof(NBossMapPoint), nameof(NBossMapPoint._Ready))];

    [HarmonyPostfix]
    public static void Postfix(NBossMapPoint __instance)
    {
        if (Act(__instance) is not InkStainedSteelSakura act)
            return;

        UsesSpine(__instance) = false;
        __instance.GetNode<Node2D>("%SpineSprite").Visible = false;
        TextureRect image = __instance.GetNode<TextureRect>("%PlaceholderImage");
        TextureRect outline = __instance.GetNode<TextureRect>("%PlaceholderOutline");
        Image(__instance) = image;
        Outline(__instance) = outline;
        image.Texture = ResourceLoader.Load<Texture2D>(InkStainedSteelSakura.MapAssetDirectory + "nagato_boss_icon.png");
        image.Material = new ShaderMaterial
        {
            Shader = ResourceLoader.Load<Shader>("res://NagatoSpire2/shaders/map/nagato_boss_icon.gdshader")
        };
        ((ShaderMaterial)image.Material).SetShaderParameter("paper_color", act.MapBgColor);
        image.Visible = true;
        image.SelfModulate = __instance.State is MapPointState.Traveled or MapPointState.Travelable
            ? act.MapTraveledColor : act.MapUntraveledColor;
        outline.Visible = false;
    }
}
