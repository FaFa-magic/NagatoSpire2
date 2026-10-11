using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

/// <summary>Temporary power takes the form of a petal stream, then scatters on impact.</summary>
public partial class NFallingPetalsVfx : Node2D
{
    public const string ScenePath = "res://NagatoSpire2/scenes/vfx/nagato_falling_petals.tscn";
    public const float GatherDuration = 0.24f;
    public const float FlightDuration = 0.32f;
    public const float ImpactTime = GatherDuration + FlightDuration;
    public const float AfterglowDuration = 0.85f;

    private static readonly Color Sakura = new(1f, 0.52f, 0.69f);
    private static readonly Color Ivory = new(1f, 0.94f, 0.91f);
    private static readonly Color Gold = new(1f, 0.79f, 0.43f);
    private static readonly Vector2[] PetalOutline =
    [
        new(0f, 0.95f), new(-0.42f, 0.35f), new(-0.64f, -0.28f),
        new(-0.42f, -0.78f), new(-0.13f, -0.94f), new(0f, -0.68f),
        new(0.16f, -0.96f), new(0.46f, -0.78f), new(0.64f, -0.26f), new(0.40f, 0.36f)
    ];

    private readonly TaskCompletionSource<bool> _arrival = new();
    private Node2D? _light;
    private Vector2 _caster;
    private Vector2 _target;
    private float _radius = 120f;
    private float _intensity;
    private float _speed = 1f;
    private float _age;
    private int _petalCount;
    private bool _initialized;
    private bool _flightStarted;
    private bool _impactCommitted;

    public Func<bool>? IsCombatActive { get; set; }
    public Func<Vector2?>? TargetPosition { get; set; }

    public void Initialize(Vector2 caster, Vector2 target, Vector2 targetSize, decimal powerStacks, float speed = 1f)
    {
        _caster = ToLocal(caster) + new Vector2(0f, -20f);
        _target = ToLocal(target);
        _radius = Mathf.Clamp(Math.Max(targetSize.X, targetSize.Y) * 0.55f, 100f, 210f);
        // Visual strength only. Never cap/recalculate the actual card damage or consume combat RNG.
        _intensity = Mathf.Clamp((float)powerStacks / 24f, 0f, 1f);
        _petalCount = 48 + (int)(_intensity * 32f);
        _speed = Math.Max(0.1f, speed);
        _initialized = true;
    }

    public Task<bool> WaitForArrivalAsync() => _arrival.Task;

    public void CommitImpact()
    {
        if (_impactCommitted || !IsInsideTree() || IsQueuedForDeletion())
            return;
        _impactCommitted = true;
        _age = ImpactTime;
        QueueRedraw();
        _light?.QueueRedraw();
    }

    public void Finish()
    {
        if (!_impactCommitted && !IsQueuedForDeletion())
            QueueFree();
    }

    public override void _Ready()
    {
        _light = new Node2D
        {
            Name = "PetalMoonlight",
            Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add }
        };
        _light.Draw += DrawLight;
        AddChild(_light);
    }

    public override void _Process(double delta)
    {
        if (!_initialized)
            return;
        if (!_impactCommitted && IsCombatActive?.Invoke() == false)
        {
            _arrival.TrySetResult(false);
            QueueFree();
            return;
        }
        if (!_impactCommitted && TargetPosition?.Invoke() is { } target)
            _target = ToLocal(target);

        _age += (float)delta * _speed;
        if (!_flightStarted && _age >= GatherDuration)
        {
            _flightStarted = true;
            // A slow frame must still show the stream rather than skipping directly to damage.
            _age = Math.Min(_age, GatherDuration + 0.02f);
        }
        if (!_impactCommitted && _age >= ImpactTime)
        {
            _age = ImpactTime;
            _arrival.TrySetResult(true);
        }
        if (_impactCommitted && _age >= ImpactTime + AfterglowDuration)
        {
            QueueFree();
            return;
        }
        QueueRedraw();
        _light?.QueueRedraw();
    }

    public override void _ExitTree() => _arrival.TrySetResult(false);

    public override void _Draw()
    {
        if (!_initialized)
            return;
        if (!_impactCommitted)
        {
            DrawGatherAndStream();
            return;
        }
        float t = _age - ImpactTime;
        float fade = Mathf.Pow(Math.Max(0f, 1f - t / AfterglowDuration), 1.5f);
        // After the brief release, the outward motion gives way to falling sakura.
        for (int i = 0; i < _petalCount; i++)
        {
            float angle = i * 2.399963f;
            Vector2 outward = Vector2.FromAngle(angle) * new Vector2(1f, 0.72f);
            float distance = (34f + Noise(i + 31) * _radius * 1.25f) * (1f - Mathf.Exp(-t * 6f));
            Vector2 p = _target + outward * distance + new Vector2(t * 18f, t * t * 125f);
            float flutter = angle + t * (1.6f + Noise(i + 17) * 3f);
            DrawPetal(p, 4.5f + Noise(i + 2) * 7f, flutter, new Color(PetalTint(i), fade * 0.92f));
        }
    }

    private void DrawGatherAndStream()
    {
        float flight = Mathf.Clamp((_age - GatherDuration) / FlightDuration, 0f, 1f);
        float entrance = Mathf.Clamp(_age / 0.16f, 0f, 1f);
        for (int i = 0; i < _petalCount; i++)
        {
            float seed = i * 2.399963f;
            float orbit = seed + _age * (2f + Noise(i + 8));
            Vector2 gathered = _caster + Vector2.FromAngle(orbit) *
                new Vector2(30f + Noise(i) * 56f, 20f + Noise(i + 6) * 48f);
            float delay = Noise(i + 11) * 0.28f;
            float p = Mathf.Clamp((flight - delay) / (1f - delay), 0f, 1f);
            Vector2 position = gathered;
            float angle = orbit;
            if (p > 0f)
            {
                // A curving, staggered stream, not a straight laser or a second iaido slash.
                position = StreamPointAt(gathered, _target, p);
                Vector2 tangent = position - StreamPointAt(gathered, _target, Math.Max(0f, p - 0.025f));
                angle = tangent.Angle() + Mathf.Pi * 0.5f + Mathf.Sin(seed + _age * 9f) * 0.5f;
                DrawLine(position, StreamPointAt(gathered, _target, Math.Max(0f, p - 0.045f)),
                    new Color(PetalTint(i), entrance * 0.18f), 1.1f, true);
            }
            DrawPetal(position, 4.5f + Noise(i + 2) * 6f, angle,
                new Color(PetalTint(i), entrance * (0.58f + p * 0.36f)));
        }
        // A few genuinely falling petals establish the card's name around the selected enemy.
        for (int i = 0; i < 9; i++)
        {
            Vector2 p = _target + new Vector2((Noise(i + 107) - 0.5f) * _radius * 2.4f,
                -_radius * (0.7f + Noise(i + 114)) + _age * (65f + Noise(i + 81) * 60f));
            DrawPetal(p, 5f + Noise(i + 45) * 5f, i + _age * 2f,
                new Color(PetalTint(i), entrance * (0.4f + flight * 0.35f)));
        }
    }

    private void DrawLight()
    {
        if (_light is null || !_initialized)
            return;
        if (!_impactCommitted)
        {
            float gather = Mathf.Clamp(_age / GatherDuration, 0f, 1f);
            float flight = Mathf.Clamp((_age - GatherDuration) / FlightDuration, 0f, 1f);
            Halo(_caster, 32f + gather * 28f, new Color(Sakura, gather * (1f - flight) * 0.13f));
            return;
        }
        float t = _age - ImpactTime;
        float pulse = Mathf.Pow(Math.Max(0f, 1f - t / 0.24f), 2f);
        Halo(_target, 42f + t * 180f, new Color(Ivory, pulse * 0.65f));
        Halo(_target, _radius * 0.55f + t * 120f, new Color(Sakura, pulse * 0.35f));

        float bloomFade = Mathf.Pow(Math.Max(0f, 1f - t / 0.44f), 1.4f);
        Vector2[] bloom = new Vector2[81];
        for (int i = 0; i < bloom.Length; i++)
        {
            float angle = i * Mathf.Tau / (bloom.Length - 1) - Mathf.Pi * 0.5f;
            float r = (26f + t * 180f) * (1f + 0.22f * Mathf.Cos((angle + Mathf.Pi * 0.5f) * 5f));
            bloom[i] = _target + Vector2.FromAngle(angle) * r * new Vector2(1f, 0.78f);
        }
        _light.DrawPolyline(bloom, new Color(Sakura, bloomFade * 0.18f), 10f, true);
        _light.DrawPolyline(bloom, new Color(Ivory, bloomFade * 0.72f), 1.5f, true);

        for (int i = 0; i < 16; i++)
        {
            float angle = i * 2.399963f;
            Vector2 direction = Vector2.FromAngle(angle);
            Vector2 p = _target + direction * (20f + t * (120f + Noise(i + 70) * 160f));
            float fade = Mathf.Pow(Math.Max(0f, 1f - t / 0.4f), 2f);
            _light.DrawLine(p, p - direction * (5f + _intensity * 10f), new Color(Gold, fade * 0.8f), 1.2f, true);
        }
    }

    private void Halo(Vector2 center, float radius, Color color)
    {
        if (_light is null || color.A <= 0f)
            return;
        for (int i = 0; i < 32; i++)
            _light.DrawPrimitive([center, center + Vector2.FromAngle(i * Mathf.Tau / 32f) * radius,
                center + Vector2.FromAngle((i + 1) * Mathf.Tau / 32f) * radius],
                [color, new Color(color, 0f), new Color(color, 0f)], []);
    }

    private void DrawPetal(Vector2 center, float radius, float angle, Color color)
    {
        float tilt = 0.48f + 0.52f * Mathf.Abs(Mathf.Cos(angle));
        Vector2[] points = new Vector2[PetalOutline.Length];
        for (int i = 0; i < points.Length; i++)
            points[i] = center + (PetalOutline[i] * new Vector2(radius * tilt, radius)).Rotated(angle);
        DrawColoredPolygon(points, color);
        DrawLine(center, center + new Vector2(0f, radius * 0.65f).Rotated(angle),
            new Color(Ivory, color.A * 0.5f), 0.8f, true);
    }

    public static Vector2 StreamPointAt(Vector2 source, Vector2 target, float progress)
    {
        float t = Mathf.Clamp(progress, 0f, 1f);
        float u = 1f - t;
        float direction = source.X <= target.X ? 1f : -1f;
        float reach = Math.Min(Math.Abs(target.X - source.X) * 0.30f, 210f);
        Vector2 a = source + new Vector2(direction * reach, -120f);
        Vector2 b = target + new Vector2(-direction * reach, -150f);
        return u * u * u * source + 3f * u * u * t * a + 3f * u * t * t * b + t * t * t * target;
    }

    private static Color PetalTint(int i) => Sakura.Lerp(Ivory, Noise(i + 43) * 0.7f);

    // Fixed local visual noise only; no combat/target RNG and no multiplayer state changes.
    private static float Noise(int i)
    {
        float value = Mathf.Sin(i * 127.1f + 311.7f) * 43758.5453f;
        return value - Mathf.Floor(value);
    }
}
