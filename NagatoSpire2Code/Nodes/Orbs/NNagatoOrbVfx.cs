using Godot;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Orbs;

public partial class NNagatoOrbVfx : NOrbVfx
{
	[Export] public float PulseScale { get; set; } = 1.14f;
	[Export] public float EvokeRotation { get; set; } = 1.8f;
	private Tween? _pulse;

	protected override bool HasFocusPower() => _orbModel is NagatoOrbModel { IsFocusAffected: true } && base.HasFocusPower();

	protected override void UpdateFocusPowerState()
	{
		base.UpdateFocusPowerState();
		GetNode<GpuParticles2D>("FocusParticles").Emitting = HasFocusPower();
	}

	public override void OnPassiveActivated(decimal passiveVal, decimal evokeVal)
	{
		base.OnPassiveActivated(passiveVal, evokeVal);
		GetNode<GpuParticles2D>("PassiveParticles").Restart();
		var core = GetNode<Node2D>("SpineSkeleton");
		_pulse?.Kill();
		_pulse = CreateTween();
		_pulse.TweenProperty(core, "scale", Vector2.One * PulseScale, 0.12).From(Vector2.One);
		_pulse.TweenProperty(core, "scale", Vector2.One, 0.32).SetTrans(Tween.TransitionType.Sine);
	}

	protected override void OnEvokeInternal(Vector2 targetVfxSpawnPosition)
	{
		if (VfxContainer == null || _orbModel == null)
			return;
		var burst = new Node2D();
		VfxContainer.AddChild(burst);
		burst.GlobalPosition = GlobalPosition;
		var particles = GetNode<GpuParticles2D>("PassiveParticles").Duplicate() as GpuParticles2D;
		if (particles != null)
		{
			particles.Amount = 36;
			particles.Lifetime = 0.8;
			var material = (ParticleProcessMaterial)particles.ProcessMaterial.Duplicate();
			material.RadialVelocityMin = 65f;
			material.RadialVelocityMax = 150f;
			particles.ProcessMaterial = material;
			burst.AddChild(particles);
			particles.Restart();
		}
		var wave = new Sprite2D
		{
			Texture = _orbModel.Icon,
			Scale = Vector2.One * 0.24f, Modulate = new Color(1f, 1f, 1f, 0.65f)
		};
		burst.AddChild(wave);
		var tween = burst.CreateTween().SetParallel();
		tween.TweenProperty(wave, "scale", Vector2.One * 0.85f, 0.65).SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(wave, "rotation", EvokeRotation, 0.65);
		tween.TweenProperty(wave, "modulate:a", 0f, 0.65);
		tween.Chain().TweenInterval(0.2);
		tween.Chain().TweenCallback(Callable.From(burst.QueueFree));
	}

	public override void _ExitTree()
	{
		_pulse?.Kill();
		base._ExitTree();
	}
}
