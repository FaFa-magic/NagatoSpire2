using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.RestSite;

[GlobalClass]
public partial class NInkStainedSteelSakuraRestSite : Control
{
    public override void _EnterTree()
    {
        foreach (string name in new[] { "SteppedFireMix", "SteppedFireAdd", "SteppedFireAdd1" })
        {
            Sprite2D fire = GetNode<Sprite2D>("RestSiteLighting/SteppedFire/" + name);
            ShaderMaterial material = (ShaderMaterial)fire.Material.Duplicate();
            foreach (string color in new[] { "OuterColor", "InnerColor" })
                material.SetShaderParameter(color, WarmColor(material.GetShaderParameter(color).AsColor()));
            fire.Material = material;
        }

        Control lighting = GetNode<Control>("%RestSiteLighting");
        foreach (Node node in lighting.FindChildren("*", "", true, false))
        {
            if (node is CpuParticles2D cpu)
                cpu.Color = WarmColor(cpu.Color);
            else if (node is GpuParticles2D gpu && gpu.ProcessMaterial is ParticleProcessMaterial particles)
            {
                ParticleProcessMaterial material = (ParticleProcessMaterial)particles.Duplicate();
                material.Color = WarmColor(material.Color);
                gpu.ProcessMaterial = material;
            }
            else if (node is Sprite2D sprite)
                sprite.SelfModulate = WarmColor(sprite.SelfModulate);
        }
    }

    public override void _Ready()
    {
        GetViewport().SizeChanged += RefreshBackground;
        RefreshBackground();
    }

    public override void _ExitTree() => GetViewport().SizeChanged -= RefreshBackground;

    private void RefreshBackground()
    {
        TextureRect background = GetNode<TextureRect>("RestSiteBG");
        Vector2 textureSize = background.Texture.GetSize();
        Vector2 viewportSize = GetViewportRect().Size;
        float scale = Mathf.Max(Mathf.Max(1920f, viewportSize.X) / textureSize.X,
            Mathf.Max(1080f, viewportSize.Y) / textureSize.Y) * 1.04f;
        background.Size = textureSize * scale;
        background.Position = new Vector2(939f, 680f) - background.Size * new Vector2(0.5f, 0.7f);
    }

    private static Color WarmColor(Color color) =>
        color.S > 0.1f ? Color.FromHsv(0.075f, color.S, color.V, color.A) : color;
}
