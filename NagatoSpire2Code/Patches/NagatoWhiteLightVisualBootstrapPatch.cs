using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

/// <summary>Power application hooks do not run when a combat save is restored.</summary>
public sealed class NagatoWhiteLightVisualBootstrapPatch : IPatchMethod
{
    public static string PatchId => "nagato_white_light_visual_bootstrap";
    public static string Description => "Restore White Light's per-creature aura after combat visuals are ready";
    public static bool IsCritical => false;
    public static ModPatchTarget[] GetTargets() => [new(typeof(NCreature), nameof(NCreature._Ready))];

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(NCreature __instance)
    {
        if (__instance.Entity is not { } creature ||
            (creature.GetPower<LastWhiteLightPower>() == null && creature.GetPower<LastWhiteLightEchoPower>() == null))
            return;
        Callable.From(() =>
        {
            if (GodotObject.IsInstanceValid(__instance) && __instance.IsInsideTree())
                NagatoWhiteLightVfx.EnsureAura(creature);
        }).CallDeferred();
    }
}
