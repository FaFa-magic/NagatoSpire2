using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

/// <summary>Persistent silver-white corona, drawn behind the creature rather than over its sprite.</summary>
public partial class NWhiteLightAuraVfx : Node2D
{
    public const string ScenePath = "res://NagatoSpire2/scenes/vfx/nagato_white_light_aura.tscn";
    public const float FadeDuration = 0.25f;

    private float _age;
    private float _fadeAge = -1f;
    private float _radius = 145f;
    private bool _initialized;

    public Func<bool>? IsAuraActive { get; set; }
    public Func<Vector2?>? OwnerPosition { get; set; }

    public void Initialize(Vector2 center, Vector2 creatureSize)
    {
        GlobalPosition = center;
        _radius = Mathf.Clamp(Math.Max(creatureSize.X, creatureSize.Y) * 0.62f, 110f, 180f);
        _initialized = true;
    }

    public void Refresh() => _fadeAge = -1f;

    public void FadeOut()
    {
        if (_fadeAge < 0f)
            _fadeAge = 0f;
    }

    public override void _Ready() =>
        Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };

    public override void _Process(double delta)
    {
        if (!_initialized)
            return;
        if (IsAuraActive?.Invoke() == false)
            FadeOut();
        if (OwnerPosition?.Invoke() is { } center)
            GlobalPosition = center;
        _age += (float)delta;
        if (_fadeAge >= 0f)
        {
            _fadeAge += (float)delta;
            if (_fadeAge >= FadeDuration)
            {
                QueueFree();
                return;
            }
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (!_initialized)
            return;
        float alpha = Mathf.Clamp(_age / 0.3f, 0f, 1f) *
            (_fadeAge < 0f ? 1f : Math.Max(0f, 1f - _fadeAge / FadeDuration));
        // A small local shimmer, not repeated full-screen white flashes.
        float pulse = 0.76f + 0.13f * Mathf.Sin(_age * 2.4f) +
            0.07f * Mathf.Sin(_age * 7.1f) + 0.04f * Mathf.Sin(_age * 11.3f);
        float radius = _radius * (1f + 0.025f * Mathf.Sin(_age * 2.4f));
        NWhiteLightSupernovaVfx.DrawHalo(this, Vector2.Zero, radius * 1.36f,
            new Color(0.83f, 0.9f, 1f, alpha * pulse * 0.30f));
        NWhiteLightSupernovaVfx.DrawHalo(this, new Vector2(0f, -radius * 0.08f), radius,
            new Color(1f, 0.98f, 0.94f, alpha * pulse * 0.42f));

        for (int i = 0; i < 28; i++)
        {
            float angle = i * Mathf.Tau / 28f + _age * 0.05f;
            Vector2 direction = Vector2.FromAngle(angle);
            float length = radius * (0.13f + 0.18f * Noise(i + 7));
            float glow = alpha * (0.12f + 0.12f * Mathf.Pow(Mathf.Sin(_age * 2f + i), 2f));
            DrawLine(direction * radius * 0.8f, direction * (radius + length),
                new Color(0.95f, 0.98f, 1f, glow), 1.2f, true);
        }
        for (int i = 0; i < 4; i++)
        {
            float start = i * Mathf.Pi * 0.5f + _age * 0.12f;
            DrawArc(Vector2.Zero, radius * 0.92f, start, start + 0.72f, 28,
                new Color(1f, 0.97f, 0.92f, alpha * pulse * 0.28f), 1.2f, true);
        }
        for (int i = 0; i < 20; i++)
        {
            float progress = (_age * (0.16f + Noise(i + 3) * 0.1f) + Noise(i + 21)) % 1f;
            Vector2 point = new((Noise(i + 12) - 0.5f) * radius * 2.1f,
                radius * (0.8f - progress * 1.85f));
            point.X += Mathf.Sin(progress * 5f + i) * 12f;
            float shimmer = Mathf.Sin(progress * Mathf.Pi) * alpha * pulse;
            float size = 1.5f + Noise(i + 34) * 2.8f;
            NWhiteLightSupernovaVfx.DrawHalo(this, point, size * 4f, new Color(0.85f, 0.94f, 1f, shimmer * 0.16f), 16);
            DrawLine(point - Vector2.Up * size, point + Vector2.Up * size,
                new Color(1f, 1f, 1f, shimmer * 0.68f), 1f, true);
            DrawLine(point - Vector2.Right * size * 0.6f, point + Vector2.Right * size * 0.6f,
                new Color(1f, 1f, 1f, shimmer * 0.45f), 1f, true);
        }
    }

    private static float Noise(int i)
    {
        float value = Mathf.Sin(i * 127.1f + 311.7f) * 43758.5453f;
        return value - Mathf.Floor(value);
    }
}
