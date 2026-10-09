using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

/// <summary>Presentation only; the stock selection screens retain ownership of every choice.</summary>
[GlobalClass]
public partial class NagatoChoiceTable : Control
{
    public const string ScenePath = "res://NagatoSpire2/scenes/ui/nagato_choice_table.tscn";
    public const string BackgroundPath = "res://NagatoSpire2/images/ui/choice/nagato_choice_table.png";
    private Control _design = null!;
    private Panel _focus = null!;
    private Tween? _focusTween;
    private readonly List<Label> _ordinals = [];
    private int _count;
    private int _focusedIndex = -1;

    public override void _Ready()
    {
        _design = GetNode<Control>("Design");
        _focus = GetNode<Panel>("Design/Focus");
        Resized += UpdateLayout;
        UpdateLayout();
    }

    public void Configure(int count, string title, string instruction)
    {
        _count = count;
        GetNode<Label>("Design/Heading").Text = title;
        GetNode<Label>("Design/Instruction").Text = instruction;
        foreach (Label label in _ordinals)
            label.QueueFree();
        _ordinals.Clear();
        for (int i = 0; i < count; i++)
        {
            var label = new Label
            {
                Text = (i + 1).ToString("00"),
                Position = OptionPosition(count, i) + new Vector2(-50f, 225f),
                Size = new Vector2(100f, 35f),
                HorizontalAlignment = HorizontalAlignment.Center,
                MouseFilter = MouseFilterEnum.Ignore
            };
            label.AddThemeColorOverride("font_color", new Color("806343"));
            label.AddThemeFontSizeOverride("font_size", 20);
            _design.AddChild(label);
            _ordinals.Add(label);
        }
    }

    public static Vector2 OptionPosition(int count, int index) =>
        new(960f + (index - (count - 1) * 0.5f) * Math.Min(390f, 1140f / Math.Max(1, count)), 548f);

    public Vector2 OptionGlobalPosition(int index) =>
        _design.GetGlobalTransform() * OptionPosition(_count, index);

    public float LayoutScale => Math.Min(Size.X / 1920f, Size.Y / 1080f);

    public bool ContainsPaperPoint(Vector2 globalPosition)
    {
        Vector2 point = _design.GetGlobalTransform().AffineInverse() * globalPosition;
        return new Rect2(240f, 235f, 1440f, 590f).HasPoint(point);
    }

    public void SetFocusedOption(int index)
    {
        if (_focusedIndex == index)
            return;
        _focusedIndex = index;
        _focusTween?.Kill();
        _focusTween = CreateTween();
        if (index >= 0 && index < _count)
        {
            _focus.Position = OptionPosition(_count, index) - _focus.Size * 0.5f;
            _focusTween.TweenProperty(_focus, "modulate:a", 1f, 0.12);
        }
        else
            _focusTween.TweenProperty(_focus, "modulate:a", 0f, 0.18);
    }

    private void UpdateLayout()
    {
        float scale = LayoutScale;
        _design.Scale = Vector2.One * scale;
        _design.Position = (Size - new Vector2(1920f, 1080f) * scale) * 0.5f;
    }
}
