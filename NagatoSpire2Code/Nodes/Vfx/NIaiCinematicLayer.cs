using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

/// <summary>Four registered drawing poses, a single directional sakura cut, and one closing verse.</summary>
public partial class NIaiCinematicLayer : Node2D
{
    public const string ReadyTexturePath = "res://NagatoSpire2/images/vfx/iai/nagato_iai_ready.png";
    public const string DrawTexturePath = "res://NagatoSpire2/images/vfx/iai/nagato_iai_draw.png";
    public const string EarlyDrawTexturePath = "res://NagatoSpire2/images/vfx/iai/nagato_iai_draw_early.png";
    public const string ReleaseTexturePath = "res://NagatoSpire2/images/vfx/iai/nagato_iai_release.png";
    public const string VerseATexturePath = "res://NagatoSpire2/images/vfx/iai/nagato_iai_verse_a.png";
    public const string VerseBTexturePath = "res://NagatoSpire2/images/vfx/iai/nagato_iai_verse_b.png";
    public const string CaptionShaderPath = "res://NagatoSpire2/shaders/vfx/iai_caption_brush.gdshader";
    public const string PoseBlendShaderPath = "res://NagatoSpire2/shaders/vfx/iai_pose_blend.gdshader";
    public const float CaptionRevealDuration = 0.50f;
    public const float CaptionDissolveDelay = 0.15f;
    public const float CaptionDissolveDuration = 0.90f;
    public const int PoseCount = 4;
    public const float DrawPoseStart = 0.99f;
    public const float DrawMotionDuration = 0.46f;
    public const float ReleasePoseStart = DrawPoseStart + DrawMotionDuration;
    public const float PathStart = 1.18f;
    public const int PathPetalCount = 88;
    public const float PortraitBottomOverscan = 0.018f;

    private static readonly Color Pink = new(1f, 0.64f, 0.78f);
    private static readonly Color Ivory = new(1f, 0.96f, 0.86f);
    private Texture2D?[] _poses = new Texture2D?[PoseCount];
    private Node2D? _portraitSurface;
    private Node2D? _drawAccentSurface;
    private ShaderMaterial? _poseMaterial;
    private Texture2D? _verse;
    private Node2D? _captionSurface;
    private ShaderMaterial? _captionMaterial;
    private float _age;
    public Shader? CaptionWipeShader { get; set; }
    public Shader? PoseBlendShader { get; set; }

    public void Configure(Texture2D? ready, Texture2D? early, Texture2D? draw,
        Texture2D? release, Texture2D? verse)
    {
        _poses = [ready, early ?? ready, draw ?? early ?? ready, release ?? draw ?? early ?? ready];
        _verse = verse;
    }

    public override void _Ready()
    {
        _poseMaterial = PoseBlendShader is null ? null : new ShaderMaterial { Shader = PoseBlendShader };
        _portraitSurface = new Node2D { Name = "RegisteredFourPoseDraw", Material = _poseMaterial };
        _portraitSurface.Draw += DrawPortraitPoses;
        AddChild(_portraitSurface);
        _drawAccentSurface = new Node2D
        {
            Name = "UnsheathingGlintsAndPetals",
            Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add }
        };
        _drawAccentSurface.Draw += DrawUnsheathingAccents;
        AddChild(_drawAccentSurface);
        _captionMaterial = CaptionWipeShader is null ? null : new ShaderMaterial { Shader = CaptionWipeShader };
        _captionSurface = new Node2D { Name = "WholeVerseBrushWipe", Material = _captionMaterial };
        _captionSurface.Draw += DrawVerse;
        AddChild(_captionSurface);
    }

    // The parent owns the only clock, including fast-mode compression and damage release.
    public void SetTime(float age)
    {
        _age = age;
        _captionMaterial?.SetShaderParameter("reveal", CaptionProgressAt(age));
        _captionMaterial?.SetShaderParameter("glow", CaptionGlowAt(age));
        _captionMaterial?.SetShaderParameter("dissolve", CaptionDissolveAt(age));
        QueueRedraw();
        _portraitSurface?.QueueRedraw();
        _drawAccentSurface?.QueueRedraw();
        _captionSurface?.QueueRedraw();
    }

    public static Rect2 PortraitRegionAt(Vector2 viewport) => new(
        viewport * new Vector2(0.50f, 0.08f), viewport * new Vector2(0.49f, 0.94f));

    public static Rect2 PortraitRectAt(Vector2 viewport, Vector2 textureSize, float age)
    {
        Rect2 region = PortraitRegionAt(viewport);
        float entrance = Smooth(0.06f, 0.44f, age);
        float push = Mathf.Clamp(age / NIaiSakuraVfx.AnticipationDuration, 0f, 1f);
        float scale = Math.Min(region.Size.X / textureSize.X, region.Size.Y / textureSize.Y) *
            (0.985f + push * 0.015f);
        Vector2 size = textureSize * scale;
        // Anchor the softened hem below the viewport instead of vertically centering the image.
        // Crop only ~1.8% of its lower fade, consistently across all registered poses/aspect ratios.
        float bottom = viewport.Y + size.Y * PortraitBottomOverscan +
            Mathf.Sin(age * 2.4f) * viewport.Y * 0.0018f;
        Vector2 center = new(viewport.X * 0.99f - size.X * 0.5f +
            (1f - entrance) * viewport.X * 0.007f, bottom - size.Y * 0.5f);
        return new Rect2(center - size * 0.5f, size);
    }

    // No pauses between intermediate keys: three equally spaced adjacent-pose transitions.
    public static float PosePositionAt(float age) => Mathf.Clamp(
        (age - DrawPoseStart) / DrawMotionDuration, 0f, 1f) * (PoseCount - 1);

    private static float AdjacentBlendAt(float position) =>
        Smooth(0.30f, 0.70f, position - Mathf.Floor(position));

    public static float PoseWeightAt(int pose, float age)
    {
        float position = PosePositionAt(age);
        int current = Math.Min((int)position, PoseCount - 1);
        if (current == PoseCount - 1)
            return pose == current ? 1f : 0f;
        float blend = AdjacentBlendAt(position);
        return pose == current ? 1f - blend : pose == current + 1 ? blend : 0f;
    }

    private static float PortraitOpacityAt(float age) => 0.80f * Smooth(0.06f, 0.34f, age) *
        (1f - Smooth(NIaiSakuraVfx.CutStart + 0.05f, NIaiSakuraVfx.CaptionStart + 0.18f, age));

    public override void _Draw()
    {
        Vector2 viewport = GetViewportRect().Size;
        DrawSetTransformMatrix(GetGlobalTransformWithCanvas().AffineInverse());
        DrawPortraitBackdrop(viewport);
        DrawSakuraPath(viewport);
        DrawSetTransformMatrix(Transform2D.Identity);
    }

    private void DrawPortraitBackdrop(Vector2 viewport)
    {
        float alpha = PortraitOpacityAt(_age);
        if (alpha <= 0f)
            return;
        // A translucent ink wash, not a hard half-screen crop or an input-blocking Control.
        float ink = alpha * 0.36f;
        DrawColoredPolygon([
            viewport * new Vector2(0.48f, 0.12f), viewport * new Vector2(1f, 0.07f),
            viewport * new Vector2(1f, 0.98f), viewport * new Vector2(0.56f, 0.92f),
            viewport * new Vector2(0.51f, 0.74f), viewport * new Vector2(0.54f, 0.39f)
        ], new Color(0.045f, 0.025f, 0.06f, ink));
    }

    private void DrawPortraitPoses()
    {
        float alpha = PortraitOpacityAt(_age);
        if (_portraitSurface is null || alpha <= 0.001f)
            return;
        Vector2 viewport = GetViewportRect().Size;
        float position = PosePositionAt(_age);
        int current = Math.Min((int)position, PoseCount - 1);
        int next = Math.Min(current + 1, PoseCount - 1);
        Texture2D? texture = _poses[current];
        if (texture is null)
            return;
        _portraitSurface.DrawSetTransformMatrix(_portraitSurface.GetGlobalTransformWithCanvas().AffineInverse());
        if (_poseMaterial is not null)
        {
            // Alpha-aware mixing in a single pass keeps the unchanged face from dimming at every key.
            _poseMaterial.SetShaderParameter("next_pose", _poses[next] ?? texture);
            // A ~61 ms adjacent blend, rather than a long double-exposed hand/sword.
            _poseMaterial.SetShaderParameter("pose_blend", AdjacentBlendAt(position));
            _portraitSurface.DrawTextureRect(texture, PortraitRectAt(viewport, texture.GetSize(), _age),
                false, new Color(1f, 1f, 1f, alpha));
        }
        else
        {
            for (int i = 0; i < PoseCount; i++)
                if (_poses[i] is { } pose && PoseWeightAt(i, _age) > 0f)
                    _portraitSurface.DrawTextureRect(pose, PortraitRectAt(viewport, pose.GetSize(), _age),
                        false, new Color(1f, 1f, 1f, alpha * PoseWeightAt(i, _age)));
        }
        _portraitSurface.DrawSetTransformMatrix(Transform2D.Identity);
    }

    private static readonly Vector2[] GuardKeys = [new(0.557f, 0.662f), new(0.447f, 0.598f),
        new(0.399f, 0.570f), new(0.235f, 0.460f)];
    private static readonly Vector2[] TipKeys = [new(0.57f, 0.67f), new(0.558f, 0.654f),
        new(0.630f, 0.678f), new(0.979f, 0.646f)];

    private static Vector2 BladePointAt(Vector2[] keys, float age)
    {
        float position = PosePositionAt(age);
        int i = Math.Min((int)position, PoseCount - 1);
        return keys[i].Lerp(keys[Math.Min(i + 1, PoseCount - 1)], position - i);
    }

    private void DrawUnsheathingAccents()
    {
        if (_drawAccentSurface is null || _poses[0] is null || _age < DrawPoseStart ||
            _age > ReleasePoseStart + 0.30f)
            return;
        Node2D canvas = _drawAccentSurface;
        Rect2 portrait = PortraitRectAt(GetViewportRect().Size, _poses[0]!.GetSize(), _age);
        float scale = portrait.Size.X / 1254f;
        float envelope = Smooth(DrawPoseStart, DrawPoseStart + 0.10f, _age) *
            (1f - Smooth(ReleasePoseStart, ReleasePoseStart + 0.22f, _age));
        canvas.DrawSetTransformMatrix(canvas.GetGlobalTransformWithCanvas().AffineInverse());
        for (int i = 0; i < 5; i++)
        {
            float age = _age - i * 0.016f;
            if (age < DrawPoseStart || age > ReleasePoseStart)
                continue;
            Vector2 a = portrait.Position + BladePointAt(GuardKeys, age) * portrait.Size;
            Vector2 b = portrait.Position + BladePointAt(TipKeys, age) * portrait.Size;
            float fade = envelope * (1f - i / 5f);
            canvas.DrawLine(a, b, new Color(Pink, fade * 0.10f), 9f * scale, true);
            canvas.DrawLine(a, b, new Color(Ivory, fade * 0.23f), 1.4f * scale, true);
        }
        // Short-lived edge glints and a few drifting sakura fragments bridge the fast draw.
        // These are local visual noise, not battle RNG and not another persistent emitter.
        for (int i = 0; i < 30; i++)
        {
            float birth = DrawPoseStart + 0.06f + Noise(i + 160) * (DrawMotionDuration - 0.08f);
            float t = (_age - birth) / (0.18f + Noise(i + 180) * 0.12f);
            if (t <= 0f || t >= 1f)
                continue;
            Vector2 a = BladePointAt(GuardKeys, birth);
            Vector2 b = BladePointAt(TipKeys, birth);
            Vector2 p = portrait.Position + a.Lerp(b, Noise(i + 190)) * portrait.Size;
            p += new Vector2(-30f * t, -16f * t + 10f * t * t) * scale;
            float fade = Mathf.Sin(t * Mathf.Pi) * (1f - t) * 0.65f;
            if (i % 3 == 0)
                DrawPetal(canvas, p, (4f + Noise(i + 200) * 4f) * scale,
                    i + t * 4f, new Color(Pink, fade));
            else
            {
                float radius = (3f + Noise(i + 210) * 4f) * scale;
                canvas.DrawLine(p - new Vector2(radius, 0), p + new Vector2(radius, 0),
                    new Color(Ivory, fade), Math.Max(0.6f, scale), true);
                canvas.DrawLine(p - new Vector2(0, radius * 0.5f), p + new Vector2(0, radius * 0.5f),
                    new Color(Pink, fade * 0.6f), Math.Max(0.6f, scale), true);
            }
        }
        canvas.DrawSetTransformMatrix(Transform2D.Identity);
    }

    public static Vector2 PathPoint(Vector2 viewport, float t)
    {
        Vector2 a = viewport * new Vector2(0.775f, 0.65f);
        Vector2 b = viewport * new Vector2(0.53f, 0.54f);
        Vector2 c = viewport * new Vector2(0.22f, 0.39f);
        Vector2 d = viewport * new Vector2(0.045f, 0.36f);
        float s = 1f - t;
        return a * (s * s * s) + b * (3f * s * s * t) + c * (3f * s * t * t) + d * (t * t * t);
    }

    private void DrawSakuraPath(Vector2 viewport)
    {
        float unit = viewport.Y / 1080f;
        float cutAge = _age - NIaiSakuraVfx.CutStart;
        if (cutAge >= 0f && cutAge < 0.34f)
        {
            float front = 1f - Mathf.Pow(1f - Mathf.Clamp(cutAge / 0.09f, 0f, 1f), 3f);
            float fade = 1f - Smooth(0.12f, 0.34f, cutAge);
            Vector2[] line = new Vector2[49];
            for (int i = 0; i < line.Length; i++)
                line[i] = PathPoint(viewport, front * i / (line.Length - 1f));
            DrawPolyline(line, new Color(Pink, fade * 0.12f), 24f * unit, true);
            DrawPolyline(line, new Color(Pink, fade * 0.38f), 8f * unit, true);
            DrawPolyline(line, new Color(Ivory, fade * 0.95f), 2.2f * unit, true);
        }

        for (int i = 0; i < PathPetalCount; i++)
        {
            float birth = PathStart + Noise(i + 20) * 0.32f;
            float t = (_age - birth) / (0.72f + Noise(i + 50) * 0.26f);
            if (t <= 0f || t >= 1f)
                continue;
            Vector2 point = PathPoint(viewport, t);
            Vector2 tangent = (PathPoint(viewport, Math.Min(1f, t + 0.01f)) -
                PathPoint(viewport, Math.Max(0f, t - 0.01f))).Normalized();
            Vector2 normal = new(-tangent.Y, tangent.X);
            point += normal * ((Noise(i) - 0.5f) * 80f + Mathf.Sin(t * 10f + i) * 16f) * unit;
            float alpha = Smooth(0f, 0.10f, t) * (1f - Smooth(0.70f, 1f, t));
            float angle = i * 2.399963f + t * 5f;
            float radius = (6f + Noise(i + 70) * 9f) * unit;
            Color color = Pink.Lerp(Ivory, Noise(i + 80) * 0.40f);
            DrawPetal(this, point, radius, angle, new Color(color, alpha * 0.95f));
            if (i % 3 == 0)
                DrawLine(point, point - tangent * radius * 2.4f, new Color(color, alpha * 0.22f), unit, true);
        }
    }

    public static float CaptionProgressAt(float age) => Mathf.Clamp(
        (age - NIaiSakuraVfx.CaptionStart) / CaptionRevealDuration, 0f, 1f);

    public static float CaptionGlowAt(float age)
    {
        float full = NIaiSakuraVfx.CaptionStart + CaptionRevealDuration;
        return Smooth(full - 0.10f, full + 0.16f, age) *
            (1f - Smooth(full + 0.28f, full + 0.80f, age));
    }

    public static float CaptionDissolveAt(float age) => Mathf.Clamp(
        (age - NIaiSakuraVfx.DamageTime - CaptionDissolveDelay) / CaptionDissolveDuration, 0f, 1f);

    private void DrawVerse()
    {
        if (_captionSurface is null || _verse is null || _age < NIaiSakuraVfx.CaptionStart)
            return;
        Vector2 viewport = GetViewportRect().Size;
        float progress = CaptionProgressAt(_age);
        float end = NIaiSakuraVfx.DamageTime + NIaiSakuraVfx.AfterglowDuration;
        float alpha = Smooth(0f, 0.10f, progress) * (1f - Smooth(end - 0.30f, end, _age));
        Vector2 available = viewport * new Vector2(0.36f, 0.76f);
        Vector2 sourceSize = _verse.GetSize();
        float scale = Math.Min(available.X / sourceSize.X, available.Y / sourceSize.Y);
        Vector2 size = sourceSize * scale;
        Vector2 center = viewport * new Vector2(0.285f, 0.52f);
        Rect2 rect = new(center - size * 0.5f, size);
        _captionSurface.DrawSetTransformMatrix(_captionSurface.GetGlobalTransformWithCanvas().AffineInverse());
        if (_captionMaterial is not null)
        {
            // One image, one continuous top-to-bottom brush front. No per-column/character reveal.
            _captionSurface.DrawTextureRect(_verse, rect, false, new Color(1f, 1f, 1f, alpha));
        }
        else if (progress > 0f)
        {
            _captionSurface.DrawTextureRectRegion(_verse,
                new Rect2(rect.Position, rect.Size * new Vector2(1f, progress)),
                new Rect2(Vector2.Zero, sourceSize * new Vector2(1f, progress)),
                new Color(1f, 1f, 1f, alpha));
        }
        _captionSurface.DrawSetTransformMatrix(Transform2D.Identity);
    }

    private static void DrawPetal(Node2D canvas, Vector2 point, float radius, float angle, Color color)
    {
        Vector2[] petal = [new(0, 0.9f), new(-0.55f, 0.15f), new(-0.62f, -0.42f),
            new(-0.26f, -0.95f), new(0, -0.70f), new(0.24f, -0.95f), new(0.64f, -0.32f), new(0.38f, 0.4f)];
        for (int j = 0; j < petal.Length; j++)
            petal[j] = point + (petal[j] * new Vector2(radius * 0.65f, radius)).Rotated(angle);
        canvas.DrawColoredPolygon(petal, color);
    }

    private static float Smooth(float start, float end, float value) =>
        Mathf.SmoothStep(0f, 1f, Mathf.Clamp((value - start) / (end - start), 0f, 1f));

    private static float Noise(int value)
    {
        float n = Mathf.Sin(value * 127.1f + 311.7f) * 43758.5453f;
        return n - Mathf.Floor(n);
    }
}
