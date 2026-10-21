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

public static class NagatoFallingPetalsVfx
{
    public const string ImpactSoundPath = "res://debug_audio/heavy_attack.mp3";

    public static async Task Play(Creature caster, Creature target, decimal strength)
    {
        if (TestMode.IsOn || NonInteractiveMode.IsActive ||
            SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant ||
            CombatManager.Instance.IsOverOrEnding || !caster.IsAlive || !target.IsAlive ||
            NCombatRoom.Instance is not { } room || room.GetCreatureNode(caster) is not { } casterNode ||
            room.GetCreatureNode(target) is not { } targetNode)
            return;

        NFallingPetalsVfx vfx = PreloadManager.Cache.GetScene(NFallingPetalsVfx.ScenePath).Instantiate<NFallingPetalsVfx>();
        room.CombatVfxContainer.AddChild(vfx);
        vfx.IsCombatActive = () => !CombatManager.Instance.IsOverOrEnding && caster.IsAlive && target.IsAlive &&
            GodotObject.IsInstanceValid(room) && ReferenceEquals(NCombatRoom.Instance, room) &&
            GodotObject.IsInstanceValid(targetNode) && targetNode.IsInsideTree();
        vfx.TargetPosition = () => GodotObject.IsInstanceValid(targetNode) ? targetNode.VfxSpawnPosition : null;
        vfx.Initialize(casterNode.VfxSpawnPosition, targetNode.VfxSpawnPosition, targetNode.Hitbox.Size, strength,
            SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast ? 1.6f : 1f);
        try
        {
            if (!await vfx.WaitForArrivalAsync() || !GodotObject.IsInstanceValid(vfx) ||
                vfx.IsQueuedForDeletion() || !vfx.IsInsideTree() || vfx.IsCombatActive?.Invoke() != true)
                return;
            vfx.CommitImpact();
            NDebugAudioManager.Instance?.Play("heavy_attack.mp3");
            NGame.Instance?.ScreenShake(strength >= 16m ? ShakeStrength.Medium : ShakeStrength.Weak, ShakeDuration.Short);
        }
        finally
        {
            if (GodotObject.IsInstanceValid(vfx))
                vfx.Finish();
        }
    }
}
