using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Nodes.Screens.TreasureRoomRelic;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

/// <summary>Skins only a Nagato choice; selection, networking, previews and confirmation remain vanilla.</summary>
public partial class NagatoChoiceScreenSkin : Node
{
    private Control _screen = null!;
    private IReadOnlyList<CardModel> _cards = [];
    private NagatoChoiceTable _table = null!;
    private NHandImage _hand = null!;
    private NPeekButton? _peek;
    private NCardGrid? _grid;
    private Control? _row;
    private bool _ownsHiddenCursor;
    private bool _handEntered;

    public static void Attach(Control screen, IReadOnlyList<CardModel> cards, bool multiple)
    {
        if (screen.HasNode("NagatoChoiceSkin"))
            return;
        var skin = new NagatoChoiceScreenSkin { Name = "NagatoChoiceSkin", _screen = screen, _cards = cards };
        screen.AddChild(skin);
        skin.Initialize(multiple);
    }

    private void Initialize(bool multiple)
    {
        _table = ResourceLoader.Load<PackedScene>(NagatoChoiceTable.ScenePath).Instantiate<NagatoChoiceTable>();
        _screen.AddChild(_table);
        _screen.MoveChild(_table, 0);
        string instruction = multiple ? "multi" : "single";
        _table.Configure(_cards.Count,
            new LocString("gameplay_ui", "NAGATO_CHOICE.title").GetFormattedText(),
            new LocString("gameplay_ui", $"NAGATO_CHOICE.{instruction}").GetFormattedText());

        _row = _screen.GetNodeOrNull<Control>("CardRow");
        _grid = _screen.GetNodeOrNull<NCardGrid>("%CardGrid");
        if (_row != null)
            foreach (NGridCardHolder holder in _row.GetChildren().OfType<NGridCardHolder>())
                holder.Modulate = Colors.White;
        if (_grid != null)
        {
            _grid.SetCanScroll(false);
            // Prevent the original grid's top/bottom masks from cutting into the desk or hovered cards.
            _grid.GetNode<Control>("ScrollContainer").ClipChildren = CanvasItem.ClipChildrenMode.Disabled;
            _grid.GetNode<CanvasItem>("BorderGradient").Visible = false;
        }
        _screen.GetNodeOrNull<CanvasItem>("Banner")?.Hide();
        _screen.GetNodeOrNull<CanvasItem>("%BottomText")?.Hide();
        _peek = _screen.GetNodeOrNull<NPeekButton>("%PeekButton");
        _peek?.AddTargets(_table);

        // Reuse the actual treasure-room hand node, including follow and press/release animations.
        _hand = NHandImage.Create(_cards[0].Owner, 0);
        _hand.Name = "NagatoChoiceHand";
        _hand.MouseFilter = Control.MouseFilterEnum.Ignore;
        _hand.ZIndex = 50;
        _hand.Visible = false;
        TextureRect texture = _hand.GetNode<TextureRect>("TextureRect");
        // Alpha-bounds measurement: Nagato's 422x1200 asset starts at fingertip (260.5, 38).
        texture.Size = new Vector2(211f, 600f);
        texture.Position = new Vector2(-130.25f, -19f);
        texture.PivotOffset = new Vector2(130.25f, 19f);
        _screen.AddChild(_hand);
        _hand.Modulate = new Color(1f, 1f, 1f, 0.94f);
        _screen.VisibilityChanged += OnVisibilityChanged;
        _screen.Resized += Layout;
        Layout();
        if (multiple)
        {
            _screen.Modulate = Colors.Transparent;
            _screen.CreateTween().TweenProperty(_screen, "modulate:a", 1f, 0.22);
        }
    }

    private IEnumerable<NGridCardHolder> Holders => _grid != null
        ? _grid.CurrentlyDisplayedCardHolders
        : _row?.GetChildren().OfType<NGridCardHolder>() ?? [];

    private void Layout()
    {
        if (!GodotObject.IsInstanceValid(_table))
            return;
        // Scale a parent slot, so the native holder's 0.8 -> 1.0 hover scale stays untouched.
        if (_row != null)
        {
            _row.Scale = Vector2.One * _table.LayoutScale;
            _row.GlobalPosition = _screen.GlobalPosition + _screen.Size * 0.5f;
        }
        else if (_grid != null)
        {
            _grid.Scale = Vector2.One * _table.LayoutScale;
        }
        _hand.Scale = Vector2.One * _table.LayoutScale;
        foreach (NGridCardHolder holder in Holders)
        {
            int index = IndexOf(holder.CardModel);
            if (index >= 0)
                holder.GlobalPosition = _table.OptionGlobalPosition(index);
        }
    }

    private int IndexOf(CardModel? model)
    {
        for (int i = 0; i < _cards.Count; i++)
            if (ReferenceEquals(model, _cards[i]))
                return i;
        return -1;
    }

    public override void _Process(double delta)
    {
        if (!GodotObject.IsInstanceValid(_table))
            return;
        // Grid initialization/reflow can replace pooled holders; never retain those references.
        Layout();
        bool active = _screen.GetWindow().HasFocus() && _screen.IsVisibleInTree() && _table.IsVisibleInTree() &&
            _peek?.IsPeeking != true && ReferenceEquals(ActiveScreenContext.Instance.GetCurrentScreen(), _screen);
        bool controller = NControllerManager.Instance?.IsUsingDirectionalNavigation == true;
        Vector2 pointer = _screen.GetGlobalMousePosition();
        int focusedIndex = -1;
        foreach (NGridCardHolder holder in Holders)
        {
            bool focused = controller ? holder.HasFocus() : holder.Hitbox.GetGlobalRect().HasPoint(pointer);
            if (focused && holder.IsVisibleInTree())
            {
                focusedIndex = IndexOf(holder.CardModel);
                if (controller)
                    pointer = holder.GlobalPosition + new Vector2(55f, 120f) * _table.LayoutScale;
                break;
            }
        }
        _table.SetFocusedOption(active ? focusedIndex : -1);
        bool showHand = active && (controller ? focusedIndex >= 0 : _table.ContainsPaperPoint(pointer));
        _hand.Visible = showHand;
        if (showHand)
        {
            _hand.SetPointingPosition(pointer);
            if (!_handEntered)
            {
                _hand.AnimateIn();
                _handEntered = true;
            }
            _hand.SetIsDown(!controller && Input.IsMouseButtonPressed(MouseButton.Left));
        }
        SetCursorHidden(showHand && !controller);
    }

    private void OnVisibilityChanged()
    {
        if (!_screen.IsVisibleInTree())
            SetCursorHidden(false);
    }

    private void SetCursorHidden(bool hidden)
    {
        if (hidden && !_ownsHiddenCursor && Input.MouseMode == Input.MouseModeEnum.Visible)
        {
            NGame.Instance?.CursorManager.SetCursorShown(false);
            _ownsHiddenCursor = true;
        }
        else if (!hidden && _ownsHiddenCursor)
        {
            NGame.Instance?.CursorManager.SetCursorShown(true);
            _ownsHiddenCursor = false;
        }
    }

    public override void _ExitTree()
    {
        SetCursorHidden(false);
        if (GodotObject.IsInstanceValid(_screen))
        {
            _screen.VisibilityChanged -= OnVisibilityChanged;
            _screen.Resized -= Layout;
        }
    }
}
