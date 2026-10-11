using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

/// <summary>Contraction, visible supernova release, then a damage cue and a lingering shockwave.</summary>
public partial class NWhiteLightSupernovaVfx : Node2D
{
    public const string ScenePath = "res://NagatoSpire2/scenes/vfx/nagato_white_light_supernova.tscn";
    public const float ChargeDuration = 0.36f;
    public const float DamageTime = 0.50f;
    public const float AfterglowDuration = 1.0f;

    private readonly TaskCompletionSource<bool> _damageCue = new();
    private Vector2 _center;
    private float _radius = 145f;
    private float _age;
    private float _speed = 1f;
    private bool _initialized;
    private bool _burstStarted;
    private bool _damageCommitted;

    public Func<bool>? IsCombatActive { get; set; }
    public Func<Vector2?>? OwnerPosition { get; set; }
    public Action? BurstStarted { get; set; }

    public void Initialize(Vector2 center, Vector2 creatureSize, float speed = 1f)
    {
        _center = ToLocal(center);
        _radius = Mathf.Clamp(Math.Max(creatureSize.X, creatureSize.Y) * 0.62f, 110f, 180f);
        _speed = Math.Max(0.1f, speed);
        _initialized = true;
    }

    public Task<bool> WaitForDamageCueAsync() => _damageCue.Task;

    public void CommitDamage()
    {
        if (_damageCommitted || !_burstStarted || !IsInsideTree() || IsQueuedForDeletion())
            return;
        _damageCommitted = true;
        _age = DamageTime;
        QueueRedraw();
    }

    public void Finish()
    {
        if (!_damageCommitted && !IsQueuedForDeletion())
            QueueFree();
    }

    public override void _Ready() =>
        Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };

    public override void _Process(double delta)
    {
        if (!_initialized)
            return;
        if (!_damageCommitted && IsCombatActive?.Invoke() == false)
        {
            _damageCue.TrySetResult(false);
            QueueFree();
            return;
        }
        if (!_burstStarted && OwnerPosition?.Invoke() is { } center)
            _center = ToLocal(center);
        _age += (float)delta * _speed;
        if (!_burstStarted && _age >= ChargeDuration)
        {
            _burstStarted = true;
            // Even a stalled frame must display the release before damage is allowed through.
            _age = ChargeDuration + 0.01f;
            BurstStarted?.Invoke();
        }
        if (!_damageCommitted && _age >= DamageTime)
        {
            _age = DamageTime;
            _damageCue.TrySetResult(true);
        }
        if (_damageCommitted && _age >= DamageTime + AfterglowDuration)
        {
            QueueFree();
            return;
        }
        QueueRedraw();
    }

    public override void _ExitTree() => _damageCue.TrySetResult(false);

    public override void _Draw()
    {
        if (!_initialized)
            return;
        if (!_burstStarted)
        {
            DrawContraction();
            return;
        }
        float t = _age - ChargeDuration;
        float flare = Mathf.Exp(-t * 7f);
        float glow = Mathf.Exp(-t * 4.2f);
        float spread = 1f - Mathf.Exp(-t * 7f);
        DrawHalo(this, _center, _radius * (1.5f + spread * 3f), new Color(0.79f, 0.87f, 1f, glow * 0.36f));
        DrawHalo(this, _center, _radius * (0.75f + spread * 2.2f), new Color(1f, 0.98f, 0.94f, glow * 0.95f));
        DrawHalo(this, _center, _radius * (0.38f + spread * 0.45f), new Color(1f, 1f, 1f, flare));

        float ringFade = Mathf.Pow(Math.Max(0f, 1f - t / 1.05f), 1.7f);
        DrawRing(_radius * (0.24f + spread * 3.45f), new Vector2(1f, 0.82f), ringFade, 8f, 1.5f);
        float second = Math.Max(0f, t - 0.09f);
        if (second > 0f)
            DrawRing(_radius * (0.25f + (1f - Mathf.Exp(-second * 5f)) * 2.9f),
                new Vector2(1f, 0.91f), ringFade * 0.6f, 4f, 1f);

        for (int i = 0; i < 32; i++)
        {
            float angle = i * 2.399963f;
            Vector2 direction = Vector2.FromAngle(angle);
            float reach = _radius * (0.85f + Noise(i + 5) * 2.8f) * (0.4f + spread * 0.6f);
            Vector2[] points = [_center + direction * _radius * 0.12f,
                _center + direction * reach + direction.Orthogonal() * (2f + flare * 5f),
                _center + direction * (reach + _radius * 0.7f),
                _center + direction * reach - direction.Orthogonal() * (2f + flare * 5f)];
            DrawPolygon(points, [new Color(1f, 1f, 1f, flare * 0.5f),
                new Color(0.88f, 0.94f, 1f, flare * 0.20f), new Color(1f, 1f, 1f, 0f),
                new Color(0.88f, 0.94f, 1f, flare * 0.20f)]);
        }
        // Fine stellar fragments remain readable after the luminous core has faded.
        for (int i = 0; i < 48; i++)
        {
            Vector2 direction = Vector2.FromAngle(i * 2.399963f);
            Vector2 point = _center + direction * _radius * (0.3f + t * (1.3f + Noise(i + 41) * 2.8f));
            float fade = Mathf.Pow(Math.Max(0f, 1f - t / (0.55f + Noise(i + 29) * 0.7f)), 1.7f);
            DrawLine(point, point - direction * (5f + fade * 12f), new Color(0.93f, 0.97f, 1f, fade * 0.8f), 1.3f, true);
        }
        DrawStar(_center, _radius * (1.3f + spread * 1.8f), flare * 0.88f);
    }

    private void DrawContraction()
    {
        float p = Mathf.Clamp(_age / ChargeDuration, 0f, 1f);
        float r = _radius * (1.1f - 0.9f * p);
        DrawHalo(this, _center, r * 1.5f, new Color(0.84f, 0.92f, 1f, 0.2f + p * 0.30f));
        DrawHalo(this, _center, r * 0.6f, new Color(1f, 1f, 1f, p * 0.8f));
        DrawRing(r, Vector2.One, 0.25f + p * 0.5f, 4f, 1f);
        for (int i = 0; i < 24; i++)
        {
            Vector2 direction = Vector2.FromAngle(i * 2.399963f + p * 0.5f);
            float distance = _radius * (1.5f + Noise(i) * 0.9f) * (1f - p);
            Vector2 point = _center + direction * distance;
            DrawLine(point, point + direction * (7f + p * 18f), new Color(0.93f, 0.98f, 1f, p * 0.65f), 1.2f, true);
        }
        DrawStar(_center, 8f + p * 18f, p * 0.9f);
    }

    private void DrawRing(float radius, Vector2 shape, float alpha, float bloomWidth, float coreWidth)
    {
        Vector2[] points = new Vector2[97];
        for (int i = 0; i < points.Length; i++)
            points[i] = _center + Vector2.FromAngle(i * Mathf.Tau / (points.Length - 1)) * radius * shape;
        DrawPolyline(points, new Color(0.68f, 0.81f, 1f, alpha * 0.14f), bloomWidth, true);
        DrawPolyline(points, new Color(0.95f, 0.99f, 1f, alpha * 0.85f), coreWidth, true);
    }

    private void DrawStar(Vector2 center, float radius, float alpha)
    {
        for (int i = 0; i < 4; i++)
        {
            Vector2 direction = Vector2.FromAngle(i * Mathf.Pi * 0.5f);
            float length = i % 2 == 0 ? radius * 1.55f : radius * 0.85f;
            DrawPolygon([center - direction.Orthogonal() * 5f, center + direction * length,
                center + direction.Orthogonal() * 5f],
                [new Color(1f, 1f, 1f, alpha), new Color(0.8f, 0.9f, 1f, 0f), new Color(1f, 1f, 1f, alpha)]);
        }
    }

    public static void DrawHalo(Node2D canvas, Vector2 center, float radius, Color color, int segments = 48)
    {
        if (color.A <= 0f)
            return;
        for (int i = 0; i < segments; i++)
            canvas.DrawPrimitive([center, center + Vector2.FromAngle(i * Mathf.Tau / segments) * radius,
                center + Vector2.FromAngle((i + 1) * Mathf.Tau / segments) * radius],
                [color, new Color(color, 0f), new Color(color, 0f)], []);
    }

    private static float Noise(int i)
    {
        float value = Mathf.Sin(i * 127.1f + 311.7f) * 43758.5453f;
        return value - Mathf.Floor(value);
    }
}
