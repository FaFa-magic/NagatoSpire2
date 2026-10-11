using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx.Utilities;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.TestSupport;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

public static class NagatoSoulbindingToriiVfx
{
    public const string ImpactSoundPath = "res://debug_audio/heavy_attack.mp3";
    public static IEnumerable<string> AssetPaths =>
        [NSoulbindingToriiVfx.ScenePath, NSoulbindingToriiVfx.TexturePath, ImpactSoundPath];

    /// <summary>Runs before the original attack damage; art and particles never change sealing/damage rules.</summary>
    public static async Task Play(Creature caster, Creature target, int sealedCards)
    {
        if (TestMode.IsOn || NonInteractiveMode.IsActive ||
            SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant ||
            CombatManager.Instance.IsOverOrEnding || !caster.IsAlive || !target.IsAlive ||
            NCombatRoom.Instance is not { } room || room.GetCreatureNode(target) is not { } targetNode)
            return;

        var vfx = PreloadManager.Cache.GetScene(NSoulbindingToriiVfx.ScenePath).Instantiate<NSoulbindingToriiVfx>();
        room.CombatVfxContainer.AddChild(vfx);
        vfx.IsCombatActive = () => !CombatManager.Instance.IsOverOrEnding && caster.IsAlive && target.IsAlive &&
            GodotObject.IsInstanceValid(room) && ReferenceEquals(NCombatRoom.Instance, room) &&
            GodotObject.IsInstanceValid(targetNode) && targetNode.IsInsideTree();
        Vector2 GroundPosition() => targetNode.Hitbox.GetGlobalTransform() *
            new Vector2(targetNode.Hitbox.Size.X * 0.5f, targetNode.Hitbox.Size.Y) + new Vector2(0f, 6f);
        vfx.TargetGroundPosition = () => GodotObject.IsInstanceValid(targetNode) && targetNode.IsInsideTree() ? GroundPosition() : null;
        Vector2 size = targetNode.Hitbox.Size * targetNode.Hitbox.GetGlobalTransform().Scale.Abs() /
            vfx.GetGlobalTransform().Scale.Abs();
        vfx.Initialize(GroundPosition(), size, sealedCards,
            SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast ? 1.6f : 1f);
        try
        {
            if (!await vfx.WaitForLandingAsync() || !GodotObject.IsInstanceValid(vfx) ||
                vfx.IsQueuedForDeletion() || !vfx.IsInsideTree() || vfx.IsCombatActive?.Invoke() != true)
                return;
            vfx.CommitImpact();
            NDebugAudioManager.Instance?.Play("heavy_attack.mp3");
            NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short);
        }
        finally
        {
            if (GodotObject.IsInstanceValid(vfx))
                vfx.Finish();
        }
    }
}
