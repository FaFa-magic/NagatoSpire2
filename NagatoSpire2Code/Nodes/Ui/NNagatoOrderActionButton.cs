using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

/// <summary>Use the game's mouse/controller input and sound, with a Nagato command plaque.</summary>
public partial class NNagatoOrderActionButton : NButton
{
    private Control _visual = null!;
    private Tween? _tween;

    public static NNagatoOrderActionButton Create(Control visual, Control parent)
    {
        var button = new NNagatoOrderActionButton
        {
            Name = visual.Name + "Button",
            Position = visual.Position,
            Size = visual.Size,
            PivotOffset = visual.Size * 0.5f,
            FocusMode = FocusModeEnum.All,
            MouseDefaultCursorShape = CursorShape.PointingHand,
            _visual = visual
        };
        parent.AddChild(button);
        visual.Reparent(button, false);
        visual.Position = Vector2.Zero;
        return button;
    }

    public override void _Ready() => ConnectSignals();
    protected override void OnFocus() { base.OnFocus(); Animate(1.04f, new Color("fff0cd")); }
    protected override void OnUnfocus() => Animate(1f, Colors.White);
    protected override void OnPress() { base.OnPress(); Animate(0.97f, new Color("c6ad86")); }
    protected override void OnRelease() => Animate(1f, Colors.White);
    protected override void OnEnable() { base.OnEnable(); _visual.Modulate = Colors.White; }
    protected override void OnDisable() { base.OnDisable(); _tween?.Kill(); Scale = Vector2.One; _visual.Modulate = new Color(1f, 1f, 1f, 0.4f); }

    private void Animate(float scale, Color tint)
    {
        if (!IsEnabled) return;
        _tween?.Kill();
        _tween = CreateTween().SetParallel();
        _tween.TweenProperty(this, "scale", Vector2.One * scale, 0.13).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        _tween.TweenProperty(_visual, "modulate", tint, 0.13);
    }

    public override void _ExitTree() { _tween?.Kill(); base._ExitTree(); }
}
