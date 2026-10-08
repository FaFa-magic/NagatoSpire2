using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

/// <summary>A single cut: suspended sakura, draw, blade, then (only when committed) impact.</summary>
public partial class NIaiSakuraVfx : Node2D
{
    public const string ScenePath = "res://NagatoSpire2/scenes/vfx/nagato_iai_sakura.tscn";
    public const string CrescentTexturePath = "res://images/vfx/slash/slash_flipbook.png";
    public const string CrescentShaderPath = "res://NagatoSpire2/shaders/vfx/iai_official_crescent.gdshader";
    public const string CutInTexturePath = "res://NagatoSpire2/images/vfx/iai/nagato_iai_cutin.png";
    public const float CutInBoxFillRatio = 1.00f;
    public const float CutInMaxOpacity = 0.80f;
    public const float CutInStart = 0.12f;
    public const float CutInFadeInDuration = 0.48f;
    public const float CutInFadeOutDuration = 0.22f;
    public const int AnticipationPetalCount = 216;
    public const float AnticipationDuration = 1.50f;
    public const float CutInSlideDuration = 1.05f;
    public const float DrawDuration = 0.12f;
    public const float CutDuration = 0.20f;
    public const float CrescentScaleMultiplier = 1.10f;
    public const float CrescentForwardOffset = 60f;
    public const float CutStart = AnticipationDuration + DrawDuration;
    public const float DamageTime = CutStart + CutDuration + 0.04f;
    public const float AfterglowDuration = 0.80f;

    private static readonly Color Sakura = new(1f, 0.62f, 0.76f);
    private static readonly Color Ivory = new(1f, 0.95f, 0.84f);
    private static readonly Color Gold = new(1f, 0.76f, 0.35f);
    private readonly TaskCompletionSource<bool> _cutFinished = new();
    private Node2D? _light;
    private Node2D? _crescent;
    private Vector2 _caster;
    private Vector2[] _targets = [];
    private float _age;
    private float _speed = 1f;
    private bool _initialized;
    private bool _cutStarted;
    private bool _impactCommitted;

    public event Action? CutStarted;
    public Func<bool>? IsCombatActive { get; set; }
    public Texture2D? CrescentTexture { get; set; }
    [Export] public Shader? CrescentShader { get; set; }
    [Export] public Texture2D? CutInTexture { get; set; }

    public void Initialize(Vector2 caster, IEnumerable<Vector2> targets, float speed = 1f)
    {
        _caster = ToLocal(caster);
        UpdateTargets(targets);
        _speed = speed;
        _initialized = true;
    }

    public void UpdateTargets(IEnumerable<Vector2> targets) => _targets = targets.Select(ToLocal).ToArray();

    public Task<bool> WaitForCutAsync() => _cutFinished.Task;

    public void CommitImpact()
    {
        if (_impactCommitted || !IsInsideTree() || IsQueuedForDeletion())
            return;
        _impactCommitted = true;
        _age = DamageTime;
    }

    // No impact if the attack was cancelled or its last target disappeared during a hook.
    public void Finish()
    {
        if (!_impactCommitted && !IsQueuedForDeletion())
            QueueFree();
    }

    public override void _Ready()
    {
        _light = new Node2D
        {
            Name = "BladeLight",
            Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add }
        };
        _light.Draw += DrawLight;
        AddChild(_light);
        _crescent = new Node2D
        {
            Name = "OfficialMoonCrescent",
            Material = new ShaderMaterial { Shader = CrescentShader }
        };
        _crescent.Draw += DrawCrescent;
        AddChild(_crescent);
    }

    public override void _Process(double delta)
    {
        if (!_initialized)
            return;
        if (!_impactCommitted && IsCombatActive?.Invoke() == false)
        {
            _cutFinished.TrySetResult(false);
            QueueFree();
            return;
        }
        _age += (float)delta * _speed;
        if (!_cutStarted && _age >= CutStart)
        {
            _cutStarted = true;
            // Even on a slow frame the fully formed crescent must be rendered at least once,
            // rather than skipping the entire short flash and immediately deducting HP.
            _age = Math.Min(_age, CutStart + 0.012f);
            CutStarted?.Invoke();
        }
        if (!_impactCommitted && _age >= DamageTime)
        {
            // The visible blade finishes first. Damage is released from this same animation clock,
            // not from a separate timer that can drift under low frame rate or fast mode.
            _age = DamageTime;
            _cutFinished.TrySetResult(true);
        }
        if (_impactCommitted && _age >= DamageTime + AfterglowDuration)
        {
            QueueFree();
            return;
        }
        QueueRedraw();
        _light?.QueueRedraw();
        _crescent?.QueueRedraw();
    }

    public override void _ExitTree()
    {
        // Leaving combat while awaiting the cut must release the card coroutine without damage.
        _cutFinished.TrySetResult(false);
    }

    public override void _Draw()
    {
        if (!_initialized || _targets.Length == 0)
            return;
        float fade = Envelope();
        Transform2D screenToLocal = GetGlobalTransformWithCanvas().AffineInverse();
        Vector2 size = GetViewportRect().Size;
        DrawColoredPolygon([
            screenToLocal * Vector2.Zero, screenToLocal * new Vector2(size.X, 0),
            screenToLocal * size, screenToLocal * new Vector2(0, size.Y)
        ], new Color(0.055f, 0.035f, 0.09f, 0.12f * fade));

        DrawCutIn(screenToLocal, size);

        // Viewport-space emission: petals cover the entire screen, independent of creature
        // positions, camera transforms, aspect ratio, or the combat VFX container's offset.
        DrawSetTransformMatrix(screenToLocal);
        for (int i = 0; i < AnticipationPetalCount; i++)
        {
            float seed = i * 2.399963f;
            float travel = _age * (75f + Noise(i + 12) * 65f);
            Vector2 p = new(
                Noise(i) * size.X + Mathf.Sin(seed + _age * 1.4f) * 34f,
                Mathf.PosMod(Noise(i + 67) * (size.Y + 100f) + travel, size.Y + 100f) - 50f);
            float scatter = Math.Max(0f, _age - CutStart);
            p += new Vector2(scatter * scatter * (90f + Noise(i + 29) * 160f), -scatter * 35f);
            float petalFade = fade * Mathf.Clamp(_age * 4f - Noise(i + 90) * 0.65f, 0f, 1f);
            Color tint = Sakura.Lerp(Ivory, Noise(i + 101) * 0.35f);
            DrawPetal(this, p, 5f + Noise(i + 34) * 10f,
                seed + _age * (0.7f + Noise(i + 8)), new Color(tint, petalFade * 0.85f));
        }
        DrawSetTransformMatrix(Transform2D.Identity);

        if (!_impactCommitted)
            return;
        float t = _age - DamageTime;
        float alpha = Mathf.Pow(Math.Max(0f, 1f - t / AfterglowDuration), 1.8f);
        foreach (Vector2 target in _targets)
        {
            for (int i = 0; i < 18; i++)
            {
                float angle = i * 2.399963f;
                float distance = (70f + Noise(i + 9) * 160f) * Mathf.Sqrt(t);
                Vector2 p = target + Vector2.FromAngle(angle) * distance + new Vector2(t * 70f, t * t * 100f);
                DrawPetal(this, p, 4f + Noise(i + 6) * 6f, angle + t * 4f, new Color(Sakura, alpha));
            }
        }
    }

    private void DrawCutIn(Transform2D screenToLocal, Vector2 viewportSize)
    {
        if (CutInTexture is null)
            return;
        float opacity = CutInOpacityAt(_age);
        if (opacity <= 0f)
            return;

        // Viewport-space placement stays in the user's lower-left region, independent of shake.
        DrawSetTransformMatrix(screenToLocal);
        DrawTextureRect(CutInTexture, CutInRectAt(viewportSize, CutInTexture.GetSize(), _age), false,
            new Color(1f, 1f, 1f, opacity));
        DrawSetTransformMatrix(Transform2D.Identity);
    }

    public static Rect2 CutInRectAt(Vector2 viewportSize, Vector2 textureSize, float age)
    {
        Rect2 region = CutInRegionAt(viewportSize);
        // Fit the annotated box, without stretching or clipping the artwork.
        float scale = Math.Min(region.Size.Y * CutInBoxFillRatio / Math.Max(1f, textureSize.Y),
            region.Size.X * CutInBoxFillRatio / Math.Max(1f, textureSize.X));
        Vector2 drawSize = textureSize * scale;
        float entrance = Mathf.Clamp((age - CutInStart) / CutInSlideDuration, 0f, 1f);
        float easedEntrance = 1f - Mathf.Pow(1f - entrance, 3f);
        float drift = Mathf.Clamp((age - CutInStart) / (AnticipationDuration - CutInStart), 0f, 1f);
        float motionScale = viewportSize.Y / 1080f;
        Vector2 center = region.GetCenter() + new Vector2(0f, -drift * 6f * motionScale);
        float minX = region.Position.X + drawSize.X * 0.5f;
        float maxX = region.End.X - drawSize.X * 0.5f;
        float minY = region.Position.Y + drawSize.Y * 0.5f;
        float maxY = region.End.Y - drawSize.Y * 0.5f;
        // A full fit can leave zero horizontal slack; roundoff may otherwise invert Clamp bounds.
        center.X = maxX <= minX ? region.GetCenter().X : Mathf.Clamp(center.X, minX, maxX);
        center.Y = maxY <= minY ? region.GetCenter().Y : Mathf.Clamp(center.Y, minY, maxY);
        // Start fully off the left edge, then travel to the settled position while fading in.
        center.X -= (1f - easedEntrance) * (center.X + drawSize.X * 0.5f + viewportSize.X * 0.02f);
        return new Rect2(center - drawSize * 0.5f, drawSize);
    }

    // Normalized from the user's blue box: the left 46% below the upper 28% of the screen.
    public static Rect2 CutInRegionAt(Vector2 viewportSize) => new(
        viewportSize * new Vector2(0.005f, 0.28f), viewportSize * new Vector2(0.455f, 0.715f));

    public static float CutInOpacityAt(float age)
    {
        float entrance = Mathf.Clamp((age - CutInStart) / CutInFadeInDuration, 0f, 1f);
        float exit = Mathf.Clamp((AnticipationDuration - age) / CutInFadeOutDuration, 0f, 1f);
        return CutInMaxOpacity * Mathf.SmoothStep(0f, 1f, entrance) * Mathf.SmoothStep(0f, 1f, exit);
    }

    private void DrawLight()
    {
        if (_light is null || !_initialized || _targets.Length == 0)
            return;
        Vector2 hilt = _caster + new Vector2(26f, 36f);
        float glint = Mathf.Clamp((_age - AnticipationDuration) / DrawDuration, 0f, 1f);
        if (_age >= AnticipationDuration && _age < CutStart)
        {
            float a = Mathf.Sin(glint * Mathf.Pi);
            Star(hilt, 16f + glint * 30f, a);
            _light.DrawLine(hilt - new Vector2(60, 0), hilt + new Vector2(60, 0), new Color(Ivory, a * 0.7f), 1.4f, true);
        }

        if (!_impactCommitted)
            return;
        float t = _age - DamageTime;
        float fade = 1f - Mathf.Clamp(t / AfterglowDuration, 0f, 1f);
        foreach (Vector2 target in _targets)
        {
            Star(target, 65f + t * 85f, Mathf.Pow(fade, 4f));
            Ellipse(target, 25f + t * 210f, new Color(Sakura, fade * fade * 0.55f));
            for (int i = 0; i < 22; i++)
            {
                float angle = i * 2.399963f;
                Vector2 direction = Vector2.FromAngle(angle) * new Vector2(1f, 0.6f);
                float distance = (80f + Noise(i + 18) * 230f) * Mathf.Sqrt(t);
                Vector2 p = target + direction * distance;
                _light.DrawLine(p, p - direction * (7f + fade * 19f), new Color(Gold, fade * fade), 1.4f, true);
            }
        }
    }

    private void DrawCrescent()
    {
        float t = _age - CutStart;
        if (_crescent is null || CrescentTexture is null || !_initialized || _targets.Length == 0 ||
            t < 0f || t >= CutDuration)
            return;
        // Reuse the actual official 3x2 slash flipbook. Begin at its fully formed moon,
        // not a slowly traced curve: snap into existence, then erode across the remaining frames.
        float progress = t / CutDuration;
        int frame = progress switch { < 0.35f => 1, < 0.55f => 2, < 0.70f => 3, < 0.85f => 4, _ => 5 };
        Vector2 cell = CrescentTexture.GetSize() / new Vector2(3, 2);
        Rect2 region = new(new Vector2(frame % 3, frame / 3) * cell, cell);
        Vector2 center = new((_targets.Min(p => p.X) + _targets.Max(p => p.X)) * 0.5f,
            _targets.Average(p => p.Y));
        // "In front" is toward the attacker, including mirrored combat layouts.
        center.X += Math.Sign(_caster.X - center.X) * CrescentForwardOffset;
        float side = Mathf.Clamp(_targets.Max(p => p.X) - _targets.Min(p => p.X) + 430f, 680f, 1080f)
            * CrescentScaleMultiplier;
        // Match Grand Finale's tilted/skewed moon silhouette, enlarged to encompass the enemy group.
        _crescent.DrawSetTransformMatrix(new Transform2D(3.403392f, Vector2.One, 1.0471972f, center));
        _crescent.DrawTextureRectRegion(CrescentTexture,
            new Rect2(new Vector2(-side * 0.5f, -side * 0.5f), new Vector2(side, side)), region, Colors.White);
        _crescent.DrawSetTransformMatrix(Transform2D.Identity);
    }

    private void Star(Vector2 p, float radius, float alpha)
    {
        if (_light is null || alpha <= 0f)
            return;
        for (int i = 0; i < 32; i++)
            _light.DrawPrimitive([p, p + Vector2.FromAngle(i * Mathf.Tau / 32f) * radius,
                p + Vector2.FromAngle((i + 1) * Mathf.Tau / 32f) * radius],
                [new Color(Gold, alpha * 0.15f), new Color(Gold, 0f), new Color(Gold, 0f)], []);
        _light.DrawColoredPolygon([
            p + new Vector2(-radius, 0), p + new Vector2(-3, -3), p + new Vector2(0, -radius * 0.42f),
            p + new Vector2(3, -3), p + new Vector2(radius, 0), p + new Vector2(3, 3),
            p + new Vector2(0, radius * 0.42f), p + new Vector2(-3, 3)
        ], new Color(Ivory, alpha));
    }

    private void Ellipse(Vector2 p, float radius, Color color)
    {
        if (_light is null)
            return;
        Vector2[] points = new Vector2[65];
        for (int i = 0; i < points.Length; i++)
            points[i] = p + Vector2.FromAngle(i * Mathf.Tau / 64f) * new Vector2(radius, radius * 0.32f);
        _light.DrawPolyline(points, color, 1.3f, true);
    }

    private static void DrawPetal(Node2D canvas, Vector2 p, float radius, float angle, Color color)
    {
        // A notched sakura petal, with a narrow seam instead of circular confetti.
        Vector2[] outline = [new(0, 0.9f), new(-0.48f, 0.25f), new(-0.65f, -0.38f),
            new(-0.38f, -0.85f), new(-0.12f, -0.95f), new(0, -0.70f), new(0.16f, -0.98f),
            new(0.48f, -0.80f), new(0.66f, -0.26f), new(0.43f, 0.36f)];
        float tilt = 0.45f + 0.55f * Mathf.Abs(Mathf.Cos(angle));
        Vector2[] shape = outline.Select(v => p + (v * new Vector2(radius * tilt, radius)).Rotated(angle)).ToArray();
        canvas.DrawColoredPolygon(shape, color);
        canvas.DrawLine(p, p + new Vector2(0f, radius * 0.6f).Rotated(angle), new Color(1f, 0.92f, 0.95f, color.A * 0.6f), 1f, true);
    }

    private float Envelope() => Mathf.Clamp(_age / 0.28f, 0f, 1f) *
        (_impactCommitted ? 1f - Mathf.Clamp((_age - DamageTime) / AfterglowDuration, 0f, 1f) : 1f);

    // Local visual noise only: never consume combat RNG or alter multiplayer outcomes.
    private static float Noise(int i)
    {
        float x = Mathf.Sin(i * 127.1f + 311.7f) * 43758.5453f;
        return x - Mathf.Floor(x);
    }
}
