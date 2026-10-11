using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using NagatoSpire2.NagatoSpire2Code.Audio;
using NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;

namespace NagatoSpire2.NagatoSpire2Code.Monsters;

[RegisterMonster]
public sealed class NagatoBoss : ModMonsterTemplate
{
    public const int PhaseHp = 500;
    public const int AttackDamage = 20;
    public const string VisualsScenePath = "res://NagatoSpire2/scenes/monsters/nagato_boss.tscn";
    private const string AuraName = "KamuiAura";
    private MoveState _reviveState = null!;
    private bool _isKamui;
    private bool _isReviving;

    public bool IsKamui => _isKamui;
    public override int MinInitialHp => PhaseHp;
    public override int MaxInitialHp => PhaseHp;
    public override MonsterAssetProfile AssetProfile => new(VisualsScenePath);
    public override IEnumerable<string> AssetPaths => base.AssetPaths.Append(NNagatoKamuiAuraVfx.ShaderPath);
    public override LocString Title => IsKamui ? L10NMonsterLookup(Id.Entry + ".kamui.name") : base.Title;
    protected override string AttackSfx => NagatoAudio.AttackEvent;
    protected override string CastSfx => NagatoAudio.CastEvent;
    public override string DeathSfx => NagatoAudio.DeathEvent;
    public override bool ShouldDisappearFromDoom => IsKamui;
    public override bool ShouldFadeAfterDeath => IsKamui;

    public override void SetupSkins(MegaSprite spine, MegaSkeleton skeleton)
    {
        skeleton.SetSkinByName("default");
        skeleton.SetSlotsToSetupPose();
    }

    protected override NCreatureVisuals? TryCreateCreatureVisuals()
    {
        NCreatureVisuals visuals = RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(VisualsScenePath);
        var aura = new NNagatoKamuiAuraVfx { Name = AuraName, Visible = IsKamui };
        visuals.AddChild(aura);
        visuals.MoveChild(aura, 0);
        return visuals;
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var attack = new MoveState("BOMBARDMENT_MOVE", AttackMove, new SingleAttackIntent(AttackDamage));
        var kamuiAttack = new MoveState("KAMUI_BOMBARDMENT_MOVE", AttackMove, new SingleAttackIntent(AttackDamage));
        _reviveState = new MoveState("KAMUI_MOVE", ReviveMove, new HealIntent())
        {
            MustPerformOnceBeforeTransitioning = true,
            FollowUpState = kamuiAttack
        };
        attack.FollowUpState = attack;
        kamuiAttack.FollowUpState = kamuiAttack;
        return new MonsterMoveStateMachine([attack, _reviveState, kamuiAttack], attack);
    }

    public override bool ShouldStopCombatFromEnding() => !IsKamui || _isReviving;
    public override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature) => creature != Creature || IsKamui;
    public override bool ShouldAllowHitting(Creature creature) => creature != Creature || !_isReviving;

    public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (creature != Creature || wasRemovalPrevented || IsKamui || _isReviving)
            return Task.CompletedTask;
        AssertMutable();
        _isReviving = true;
        SetMoveImmediate(_reviveState, forceTransition: true);
        return Task.CompletedTask;
    }

    private Task AttackMove(IReadOnlyList<Creature> targets) => DamageCmd.Attack(AttackDamage)
        .FromMonster(this)
        .WithAttackerAnim("Attack", 0f)
        .WithAttackerFx(null, NagatoAudio.AttackEvent)
        .WithHitFx("vfx/vfx_attack_blunt")
        .Execute(null);

    private async Task ReviveMove(IReadOnlyList<Creature> targets)
    {
        AssertMutable();
        if (Creature.GetCreatureNode() is { } node)
        {
            if (node.DeathAnimationTask is { } deathAnimation)
                await deathAnimation;
            node.Visuals.GetNode<NNagatoKamuiAuraVfx>(AuraName).Ignite();
        }
        await Cmd.Wait(0.8f);
        _isKamui = true;
        decimal hp = Creature.ScaleHpForMultiplayer(PhaseHp, CombatState.Encounter, CombatState.Players.Count, CombatState.RunState.CurrentActIndex);
        await CreatureCmd.SetMaxHp(Creature, hp);
        await CreatureCmd.Heal(Creature, hp);
        _isReviving = false;
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        var idle = new AnimState("normal", isLooping: true);
        var attack = new AnimState("attack") { NextState = idle };
        var cast = new AnimState("attack_left") { NextState = idle };
        var hit = new AnimState("touch") { NextState = idle };
        var dead = new AnimState("dead");
        var revive = new AnimState("skill") { NextState = idle };
        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Cast", cast);
        animator.AddAnyState("Hit", hit);
        animator.AddAnyState("Dead", dead);
        animator.AddAnyState("Revive", revive);
        return animator;
    }
}
