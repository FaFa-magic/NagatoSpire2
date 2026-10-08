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

public static class NagatoIaiVfx
{
    public static NIaiSakuraVfx? Create(Creature caster, IEnumerable<Creature> targets)
    {
        // Instant and noninteractive simulations must not wait for a rendered scene.
        if (TestMode.IsOn || NonInteractiveMode.IsActive ||
            SaveManager.Instance.PrefsSave.FastMode == FastModeType.Instant ||
            NCombatRoom.Instance is not { } room || room.GetCreatureNode(caster) is not { } casterNode)
            return null;

        Vector2[] positions = Positions(targets);
        if (positions.Length == 0)
            return null;
        NIaiSakuraVfx vfx = PreloadManager.Cache.GetScene(NIaiSakuraVfx.ScenePath).Instantiate<NIaiSakuraVfx>();
        vfx.CrescentTexture = PreloadManager.Cache.GetAsset<Texture2D>(NIaiSakuraVfx.CrescentTexturePath);
        room.CombatVfxContainer.AddChild(vfx);
        vfx.IsCombatActive = () => !CombatManager.Instance.IsOverOrEnding && caster.IsAlive;
        vfx.Initialize(casterNode.VfxSpawnPosition, positions,
            SaveManager.Instance.PrefsSave.FastMode == FastModeType.Fast ? 1.6f : 1f);
        vfx.CutStarted += () =>
        {
            if (!CombatManager.Instance.IsOverOrEnding)
            {
                NDebugAudioManager.Instance?.Play("slash_attack.mp3");
                NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Short, 8f);
            }
        };
        return vfx;
    }

    public static void Impact(NIaiSakuraVfx? vfx, IEnumerable<Creature> targets)
    {
        if (vfx is null || !GodotObject.IsInstanceValid(vfx) || !vfx.IsInsideTree() || vfx.IsQueuedForDeletion())
            return;
        vfx.UpdateTargets(Positions(targets));
        vfx.CommitImpact();
        // Respect the player's existing screen-shake preference; no global time-scale/hitpause.
        NGame.Instance?.ScreenShake(ShakeStrength.Strong, ShakeDuration.Normal, 8f);
    }

    public static void Finish(NIaiSakuraVfx? vfx)
    {
        if (vfx is not null && GodotObject.IsInstanceValid(vfx))
            vfx.Finish();
    }

    private static Vector2[] Positions(IEnumerable<Creature> creatures) => creatures
        .Where(c => c.IsAlive)
        .Select(c => NCombatRoom.Instance?.GetCreatureNode(c))
        .Where(n => n is not null)
        .Select(n => n!.VfxSpawnPosition).ToArray();
}
