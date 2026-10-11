using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

/// <summary>A rigid shrine gate accelerates out of the sky and plants into a soul-binding seal.</summary>
public partial class NSoulbindingToriiVfx : Node2D
{
    public const string ScenePath = "res://NagatoSpire2/scenes/vfx/nagato_soulbinding_torii.tscn";
    public const string TexturePath = "res://NagatoSpire2/images/vfx/soulbinding_torii/nagato_soulbinding_torii.png";
    public const float SummonDuration = 0.22f;
    public const float FallDuration = 0.58f;
    public const float ImpactTime = SummonDuration + FallDuration;
    public const float AfterglowDuration = 0.85f;

    private static readonly Color Gold = new(1f, 0.78f, 0.43f);
    private static readonly Color Ivory = new(1f, 0.96f, 0.88f);
    private readonly TaskCompletionSource<bool> _arrival = new();
    private Sprite2D? _gate;
    private Node2D? _light;
    private Vector2 _landing;
    private Vector2 _gateSize;
    private Vector2 _spriteScale;
    private float _fallDistance;
    private float _age;
    private float _speed = 1f;
    private float _intensity;
    private bool _initialized;
    private bool _fallStarted;
    private bool _descentShown;
    private bool _impactCommitted;

    [Export] public Texture2D? ToriiTexture { get; set; }
    [Export] public Rect2 TexturePixelBounds { get; set; }
    public Func<bool>? IsCombatActive { get; set; }
    public Func<Vector2?>? TargetGroundPosition { get; set; }

    public void Initialize(Vector2 worldLanding, Vector2 targetSize, int sealedCards, float speed = 1f)
    {
        if (_gate?.Texture is not { } texture)
            throw new InvalidOperationException("Soulbinding Torii VFX requires its gate texture.");
        _landing = ToLocal(worldLanding);
        _intensity = Mathf.Clamp(sealedCards / 12f, 0f, 1f);
        float width = Mathf.Clamp(Math.Max(targetSize.X * 1.45f, targetSize.Y * 1.35f), 340f, 540f);
        Rect2 region = TexturePixelBounds.HasArea() ? TexturePixelBounds : new Rect2(Vector2.Zero, texture.GetSize());
        _gate.RegionEnabled = true;
        _gate.RegionRect = region;
        // Uniform scaling preserves the generated architecture; never stretch the posts or crossbeam.
        _spriteScale = Vector2.One * (width / region.Size.X);
        _gateSize = region.Size * _spriteScale;
        Vector2 top = GetGlobalTransformWithCanvas().AffineInverse() * new Vector2(0f, -100f);
        _fallDistance = Math.Max(_gateSize.Y * 1.9f, _landing.Y - top.Y);
        _speed = Math.Max(0.1f, speed);
        _initialized = true;
        UpdateGate();
    }

    public Task<bool> WaitForLandingAsync() => _arrival.Task;

    public void CommitImpact()
    {
        if (_impactCommitted || !IsInsideTree() || IsQueuedForDeletion())
            return;
        _impactCommitted = true;
        _age = ImpactTime;
        UpdateGate();
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
            Name = "SoulbindingLight",
            Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add }
        };
        _light.Draw += DrawLight;
        AddChild(_light);
        _gate = new Sprite2D { Name = "DescendingTorii", Texture = ToriiTexture, Visible = false };
        AddChild(_gate);
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
        if (!_impactCommitted && TargetGroundPosition?.Invoke() is { } ground)
            _landing = ToLocal(ground);
        _age += (float)delta * _speed;
        if (!_fallStarted && _age >= SummonDuration)
        {
            _fallStarted = true;
            // A slow frame must not skip the descent and deal damage without showing a falling gate.
            _age = Math.Min(_age, SummonDuration + 0.02f);
        }
        if (!_descentShown && _age >= SummonDuration + FallDuration * 0.84f)
        {
            _descentShown = true;
            // Keep one actually visible airborne frame, not just an off-screen start, before landing.
            _age = Math.Min(_age, SummonDuration + FallDuration * 0.84f);
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
        UpdateGate();
        QueueRedraw();
        _light?.QueueRedraw();
    }

    public override void _ExitTree() => _arrival.TrySetResult(false);

    private void UpdateGate()
    {
        if (_gate is null || !_initialized)
            return;
        float fall = Mathf.Clamp((_age - SummonDuration) / FallDuration, 0f, 1f);
        float after = _age - ImpactTime;
        float opacity = _impactCommitted ? 1f - Mathf.SmoothStep(0.16f, 0.74f, after) : Mathf.Clamp(fall / 0.12f, 0f, 1f);
        float settling = _impactCommitted ? Mathf.Sin(after * 24f) * Mathf.Exp(-after * 15f) * 3f : 0f;
        _gate.Visible = _age >= SummonDuration;
        _gate.Position = GateFootPosition(fall) - new Vector2(0f, _gateSize.Y * 0.5f) + new Vector2(0f, settling);
        _gate.Scale = _spriteScale;
        _gate.Modulate = new Color(1f, 1f, 1f, opacity);
    }

    private Vector2 GateFootPosition(float fall) => _landing -
        new Vector2(0f, _fallDistance * (1f - FallProgress(fall)));

    public static float FallProgress(float progress) => Mathf.Pow(Mathf.Clamp(progress, 0f, 1f), 2.8f);

    public override void _Draw()
    {
        if (!_initialized)
            return;
        float radius = _gateSize.X * 0.53f;
        float fall = Mathf.Clamp((_age - SummonDuration) / FallDuration, 0f, 1f);
        float after = Math.Max(0f, _age - ImpactTime);
        float fade = _impactCommitted ? Math.Max(0f, 1f - after / AfterglowDuration) : fall;
        // A growing contact shadow makes the gate read as heavy architecture rather than an icon overlay.
        Vector2[] shadow = new Vector2[49];
        for (int i = 0; i < shadow.Length; i++)
            shadow[i] = _landing + Vector2.FromAngle(i * Mathf.Tau / (shadow.Length - 1)) *
                new Vector2(radius * (0.45f + fall * 0.55f), radius * 0.12f);
        DrawColoredPolygon(shadow, new Color(0.035f, 0.012f, 0.025f, fade * 0.3f));
        if (!_impactCommitted)
            return;
        float scatter = 1f - Mathf.Exp(-after * 7f);
        int count = 26 + (int)(_intensity * 16f);
        for (int i = 0; i < count; i++)
        {
            float side = i % 2 == 0 ? -1f : 1f;
            float reach = radius * (0.2f + Noise(i + 18) * 0.9f);
            Vector2 point = _landing + new Vector2(side * (radius * 0.38f + reach * scatter),
                -Mathf.Sin(Mathf.Clamp(after / 0.72f, 0f, 1f) * Mathf.Pi) * (15f + Noise(i + 33) * 75f));
            float angle = i + after * (side * 3f);
            if (i % 3 == 0)
            {
                // Ivory paper fragments echo the shide and the card's sealing cords.
                Vector2[] paper = [new(-3, -7), new(4, -7), new(4, 6), new(-3, 7)];
                for (int j = 0; j < paper.Length; j++)
                    paper[j] = point + paper[j].Rotated(angle);
                DrawColoredPolygon(paper, new Color(Ivory, fade * 0.82f));
                DrawLine(point - Vector2.FromAngle(angle) * 2f, point + Vector2.FromAngle(angle) * 2f,
                    new Color(0.63f, 0.10f, 0.15f, fade * 0.5f), 1f, true);
            }
            else
            {
                float size = 2f + Noise(i + 14) * 5f;
                Vector2[] chip = [new(-size, 0f), new(-size * 0.3f, -size), new(size, 0f), new(size * 0.6f, size * 0.7f)];
                for (int j = 0; j < chip.Length; j++)
                    chip[j] = point + chip[j].Rotated(angle);
                DrawColoredPolygon(chip, new Color(new Color(0.63f, 0.55f, 0.43f).Lerp(Ivory, Noise(i)), fade * 0.75f));
            }
        }
    }

    private void DrawLight()
    {
        if (_light is null || !_initialized)
            return;
        float radius = _gateSize.X * 0.53f;
        float summon = Mathf.Clamp(_age / SummonDuration, 0f, 1f);
        if (!_impactCommitted)
        {
            float fall = Mathf.Clamp((_age - SummonDuration) / FallDuration, 0f, 1f);
            float rotation = _age * 0.35f;
            DrawSeal(radius * 0.81f, summon * 0.65f, rotation);
            if (fall > 0f)
            {
                Vector2 foot = GateFootPosition(fall);
                float alpha = Mathf.Clamp(fall * 1.4f, 0f, 0.8f);
                foreach (float side in new[] { -1f, 1f })
                {
                    Vector2 point = foot + new Vector2(side * _gateSize.X * 0.32f, -_gateSize.Y * 0.25f);
                    _light.DrawLine(point, point - new Vector2(0f, 100f + fall * 260f), new Color(Gold, alpha * 0.10f), 14f, true);
                    _light.DrawLine(point, point - new Vector2(0f, 70f + fall * 220f), new Color(Ivory, alpha * 0.65f), 1.6f, true);
                }
            }
            return;
        }
        float t = _age - ImpactTime;
        float pulse = Mathf.Exp(-t * 12f);
        float glow = Mathf.Exp(-t * 5f);
        float spread = 1f - Mathf.Exp(-t * 7f);
        float fade = Mathf.Pow(Math.Max(0f, 1f - t / AfterglowDuration), 1.7f);
        DrawSeal(radius * 0.81f, fade, 0.28f);
        DrawEllipse(radius * (0.6f + spread * 1.8f), 0.28f, new Color(Gold, fade * 0.8f), 7f, 1.5f);
        if (t > 0.07f)
            DrawEllipse(radius * (0.45f + (1f - Mathf.Exp(-(t - 0.07f) * 6f)) * 1.5f), 0.30f,
                new Color(Ivory, fade * 0.7f), 3f, 1f);
        foreach (float side in new[] { -1f, 1f })
        {
            Vector2 foot = _landing + new Vector2(side * _gateSize.X * 0.32f, 0f);
            Halo(foot, 58f + t * 110f, new Color(Gold, glow * 0.55f));
            Halo(foot, 36f + t * 30f, new Color(Ivory, pulse * 0.9f));
        }
        Halo(_landing - new Vector2(0f, _gateSize.Y * 0.37f), radius * 0.9f,
            new Color(Ivory, pulse * 0.28f));
        for (int i = 0; i < 22; i++)
        {
            Vector2 direction = Vector2.FromAngle(i * 2.399963f) * new Vector2(1f, 0.32f);
            Vector2 point = _landing + direction * radius * (0.4f + spread * (0.7f + Noise(i + 4)));
            _light.DrawLine(point, point - direction * (8f + glow * 32f), new Color(Ivory, fade * 0.7f), 1.3f, true);
        }
    }

    private void DrawSeal(float radius, float alpha, float rotation)
    {
        if (_light is null)
            return;
        DrawEllipse(radius, 0.28f, new Color(Gold, alpha * 0.8f), 5f, 1f);
        DrawEllipse(radius * 0.78f, 0.28f, new Color(Ivory, alpha * 0.6f), 3f, 0.8f);
        for (int i = 0; i < 8; i++)
        {
            float angle = rotation + i * Mathf.Tau / 8f;
            Vector2 direction = Vector2.FromAngle(angle) * new Vector2(1f, 0.28f);
            Vector2 point = _landing + direction * radius * 0.88f;
            _light.DrawLine(point - direction * 7f, point + direction * 7f, new Color(Ivory, alpha), 1.1f, true);
        }
    }

    private void DrawEllipse(float radius, float flatten, Color color, float glowWidth, float coreWidth)
    {
        if (_light is null)
            return;
        Vector2[] points = new Vector2[81];
        for (int i = 0; i < points.Length; i++)
            points[i] = _landing + Vector2.FromAngle(i * Mathf.Tau / (points.Length - 1)) * new Vector2(radius, radius * flatten);
        _light.DrawPolyline(points, new Color(color, color.A * 0.12f), glowWidth, true);
        _light.DrawPolyline(points, color, coreWidth, true);
    }

    private void Halo(Vector2 center, float radius, Color color)
    {
        if (_light is null)
            return;
        for (int i = 0; i < 32; i++)
            _light.DrawPrimitive([center, center + Vector2.FromAngle(i * Mathf.Tau / 32f) * radius,
                center + Vector2.FromAngle((i + 1) * Mathf.Tau / 32f) * radius],
                [color, new Color(color, 0f), new Color(color, 0f)], []);
    }

    private static float Noise(int i)
    {
        float value = Mathf.Sin(i * 127.1f + 311.7f) * 43758.5453f;
        return value - Mathf.Floor(value);
    }
}
