using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

public partial class NNagatoKamuiAuraVfx : Node2D
{
    public const string ShaderPath = "res://NagatoSpire2/shaders/vfx/nagato_kamui_aura.gdshader";
    private float _age;

    public override void _Ready()
    {
        Shader shader = ResourceLoader.Load<Shader>(ShaderPath);
        AddLayer(shader, false);
        AddLayer(shader, true);
        Modulate = new Color(1f, 1f, 1f, 0f);
    }

    public void Ignite()
    {
        _age = 0f;
        Visible = true;
    }

    public override void _Process(double delta)
    {
        if (!Visible)
            return;
        _age += (float)delta;
        Modulate = new Color(1f, 1f, 1f, Mathf.Clamp(_age / 0.8f, 0f, 1f));
    }

    private void AddLayer(Shader shader, bool flames)
    {
        var material = new ShaderMaterial { Shader = shader };
        material.SetShaderParameter("flames", flames);
        AddChild(new ColorRect
        {
            Position = new Vector2(-260f, -400f),
            Size = new Vector2(520f, 450f),
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Material = material
        });
    }
}
