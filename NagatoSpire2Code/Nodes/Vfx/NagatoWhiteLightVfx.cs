using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

public static class NagatoWhiteLightVfx
{
    public const string BurstSoundPath = "res://debug_audio/heavy_attack.mp3";
    public static IEnumerable<string> AssetPaths =>
        [NWhiteLightAuraVfx.ScenePath, NWhiteLightSupernovaVfx.ScenePath, BurstSoundPath];

    private static bool SkipRendering => TestMode.IsOn || NonInteractiveMode.IsActive ||
        SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant;

    private static bool HasLight(Creature creature) => creature.GetPower<LastWhiteLightPower>() != null ||
        creature.GetPower<LastWhiteLightEchoPower>() != null;

    private static string AuraName(NCreature creature) => $"NagatoWhiteLightAura_{creature.GetInstanceId()}";

    public static void EnsureAura(Creature creature)
    {
        if (SkipRendering || !creature.IsAlive || !HasLight(creature) || CombatManager.Instance.IsOverOrEnding ||
            NCombatRoom.Instance is not { } room || room.GetCreatureNode(creature) is not { } node)
            return;

        string name = AuraName(node);
        var existing = room.BackCombatVfxContainer.GetNodeOrNull<NWhiteLightAuraVfx>(name);
        if (existing != null && !existing.IsQueuedForDeletion())
        {
            existing.Refresh();
            return;
        }
        // A fading node scheduled for deletion must not force Godot to rename its replacement.
        if (existing != null)
            existing.Name = name + "_expired";
        var aura = PreloadManager.Cache.GetScene(NWhiteLightAuraVfx.ScenePath).Instantiate<NWhiteLightAuraVfx>();
        aura.Name = name;
        room.BackCombatVfxContainer.AddChild(aura);
        aura.IsAuraActive = () => creature.IsAlive && HasLight(creature) && !CombatManager.Instance.IsOverOrEnding &&
            GodotObject.IsInstanceValid(room) && ReferenceEquals(NCombatRoom.Instance, room) &&
            GodotObject.IsInstanceValid(node) && node.IsInsideTree();
        aura.OwnerPosition = () => GodotObject.IsInstanceValid(node) && node.IsInsideTree() ? node.VfxSpawnPosition : null;
        aura.Initialize(node.VfxSpawnPosition, LocalCreatureSize(node, aura));
    }

    public static void FadeAura(Creature creature)
    {
        if (SkipRendering || NCombatRoom.Instance is not { } room || room.GetCreatureNode(creature) is not { } node)
            return;
        room.BackCombatVfxContainer.GetNodeOrNull<NWhiteLightAuraVfx>(AuraName(node))?.FadeOut();
    }

    /// <summary>The original power removal runs exactly once, including non-rendered combat simulations.</summary>
    public static async Task Detonate(Creature creature, Func<Task> removePower)
    {
        if (SkipRendering || !creature.IsAlive || CombatManager.Instance.IsOverOrEnding ||
            NCombatRoom.Instance is not { } room || room.GetCreatureNode(creature) is not { } node)
        {
            await removePower();
            return;
        }

        FadeAura(creature);
        var vfx = PreloadManager.Cache.GetScene(NWhiteLightSupernovaVfx.ScenePath).Instantiate<NWhiteLightSupernovaVfx>();
        room.CombatVfxContainer.AddChild(vfx);
        vfx.IsCombatActive = () => creature.IsAlive && !CombatManager.Instance.IsOverOrEnding &&
            GodotObject.IsInstanceValid(room) && ReferenceEquals(NCombatRoom.Instance, room) &&
            GodotObject.IsInstanceValid(node) && node.IsInsideTree();
        vfx.OwnerPosition = () => GodotObject.IsInstanceValid(node) && node.IsInsideTree() ? node.VfxSpawnPosition : null;
        vfx.BurstStarted = () =>
        {
            NDebugAudioManager.Instance?.Play("heavy_attack.mp3");
            NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);
        };
        vfx.Initialize(node.VfxSpawnPosition, LocalCreatureSize(node, vfx),
            SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast ? 1.6f : 1f);
        try
        {
            // Overlap the official removal's short animation wait with contraction, not with the damage frame.
            await removePower();
            if (!await vfx.WaitForDamageCueAsync() || !GodotObject.IsInstanceValid(vfx) ||
                vfx.IsQueuedForDeletion() || !vfx.IsInsideTree() || vfx.IsCombatActive?.Invoke() != true)
                return;
            vfx.CommitDamage();
        }
        finally
        {
            if (GodotObject.IsInstanceValid(vfx))
                vfx.Finish();
        }
    }

    private static Vector2 LocalCreatureSize(NCreature node, CanvasItem vfx) =>
        node.Hitbox.Size * node.Hitbox.GetGlobalTransform().Scale.Abs() / vfx.GetGlobalTransform().Scale.Abs();
}
