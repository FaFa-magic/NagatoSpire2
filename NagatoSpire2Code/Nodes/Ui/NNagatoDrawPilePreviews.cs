using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using NagatoSpire2.NagatoSpire2Code.Characters;
using NagatoSpire2.NagatoSpire2Code.Combat;
using STS2RitsuLib;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

public partial class NNagatoDrawPilePreviews : Control
{
    private const string CardScenePath = "res://scenes/cards/card.tscn";
    private const float CardScale = 0.45f;
    private const float VisualMargin = 24f;
    private const float LayoutDuration = 0.32f;

    private sealed class Entry(CardModel model, Control clip, NCard node)
    {
        public CardModel Model { get; } = model;
        public Control Clip { get; } = clip;
        public NCard Node { get; } = node;
        public Tween? Tween { get; set; }
        public Action? Refresh { get; set; }
    }

    public readonly record struct Addition(NNagatoDrawPilePreviews View, uint Version);

    private static NNagatoDrawPilePreviews? _active;
    private readonly NagatoKnownDrawOrder<CardModel> _known = new();
    private readonly List<Entry> _entries = [];
    private NDrawPileButton _button = null!;
    private CardPile _pile = null!;
    private Player _player = null!;
    private Vector2 _viewportSize;
    private bool _expanded;
    private bool _failed;

    public static void Attach(NDrawPileButton button, Player player)
    {
        if (player.Character is not NagatoCharacter || !LocalContext.IsMe(player) ||
            player.PlayerCombatState is not { } state || button.GetParent() is not Control parent)
            return;

        if (ForPile(state.DrawPile) is { } current && current._button == button)
            return;
        if (GodotObject.IsInstanceValid(_active))
        {
            _active!.Clear();
            _active.QueueFree();
        }
        var view = new NNagatoDrawPilePreviews
        {
            Name = "NagatoDrawPilePreviews",
            MouseFilter = MouseFilterEnum.Ignore,
            _button = button,
            _pile = state.DrawPile,
            _player = player
        };
        _active = view;
        parent.AddChild(view);
        view.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        parent.MoveChild(view, button.GetIndex());
    }

    private static NNagatoDrawPilePreviews? ForPile(CardPile pile) =>
        GodotObject.IsInstanceValid(_active) && _active!.IsInsideTree() && !_active.IsQueuedForDeletion() &&
        !_active._failed && ReferenceEquals(_active._pile, pile) ? _active : null;

    public static Addition? BeginAddition(CardPile pile, CardPilePosition position)
    {
        var view = ForPile(pile);
        if (view is null)
            return null;
        if (position == CardPilePosition.Random)
            view._known.Invalidate();
        return position == CardPilePosition.Top ? new Addition(view, view._known.Version) : null;
    }

    public static void CompleteAddition(Addition addition, IReadOnlyList<CardPileAddResult> results)
    {
        var view = addition.View;
        if (!GodotObject.IsInstanceValid(view) || !ReferenceEquals(ForPile(view._pile), view) ||
            view._known.Version != addition.Version)
            return;
        view._known.Remember(results.Where(result => result.success && ReferenceEquals(result.cardAdded.Pile, view._pile))
            .Select(result => result.cardAdded));
    }

    public static void Forget(CardPile pile, CardModel card) => ForPile(pile)?._known.Forget(card);

    public static void Invalidate(CardPile pile) => ForPile(pile)?._known.Invalidate();

    public static void RememberTop(CardPile pile, CardModel card) => ForPile(pile)?._known.Remember([card]);

    public static void RememberOrder(CardPile pile) => ForPile(pile)?._known.Remember(pile.Cards);

    public static void RememberTransformation(CardModel original, CardModel? replacement)
    {
        if (replacement is not null && original.Pile is { } pile && ForPile(pile) is { } view && view._known.Contains(original))
            view._known.Remember([replacement]);
    }

    public override void _Process(double delta)
    {
        if (_failed)
            return;
        if (!GodotObject.IsInstanceValid(_button) || !ReferenceEquals(_player.PlayerCombatState?.DrawPile, _pile) ||
            CombatManager.Instance.IsOverOrEnding || _player.Creature.IsDead)
        {
            Clear();
            Hide();
            return;
        }
        Visible = _button.IsVisibleInTree();
        if (!Visible)
            return;
        try
        {
            var cards = _known.GetTop(_pile.Cards);
            bool changed = !_entries.Select(entry => entry.Model).SequenceEqual(cards);
            if (changed)
                Refresh(cards);
            Vector2 size = GetViewportRect().Size;
            bool hotkey = InputMap.HasAction(MegaInput.viewDrawPile) && Input.IsActionPressed(MegaInput.viewDrawPile);
            bool expanded = GetWindow().HasFocus() && (_button.HasFocus() || hotkey ||
                _button.GetGlobalRect().Grow(60f).HasPoint(GetGlobalMousePosition()));
            if (changed || size != _viewportSize || expanded != _expanded)
            {
                _viewportSize = size;
                _expanded = expanded;
                Layout(true);
            }
        }
        catch (Exception exception)
        {
            Disable(exception);
        }
    }

    private void Refresh(CardModel[] cards)
    {
        foreach (var entry in _entries.Where(entry => !cards.Contains(entry.Model)).ToArray())
        {
            Remove(entry);
            _entries.Remove(entry);
        }
        for (int i = 0; i < cards.Length; i++)
        {
            var entry = _entries.FirstOrDefault(entry => ReferenceEquals(entry.Model, cards[i]));
            if (entry is null)
            {
                var node = PreloadManager.Cache.GetScene(CardScenePath).Instantiate<NCard>();
                node.Model = cards[i];
                node.Scale = Vector2.One * CardScale;
                var clip = new Control { MouseFilter = MouseFilterEnum.Ignore, ClipContents = true };
                entry = new Entry(cards[i], clip, node);
                _entries.Insert(i, entry);
                AddChild(clip);
                clip.AddChild(node);
                node.MouseFilter = MouseFilterEnum.Ignore;
                foreach (var control in node.FindChildren("*", "Control", true, false).OfType<Control>())
                    control.MouseFilter = MouseFilterEnum.Ignore;
                node.UpdateVisuals(PileType.Draw, CardPreviewMode.Normal);
                Subscribe(entry);
                clip.GlobalPosition = ClipPosition(Center(i) - Vector2.Right * ScaledSize.X);
            }
            else
            {
                _entries.Remove(entry);
                _entries.Insert(i, entry);
            }
        }
    }

    private static Vector2 ScaledSize => NCard.defaultSize * CardScale;

    private Vector2 Center(int index)
    {
        var viewport = GetViewportRect();
        float y = viewport.End.Y - (_expanded ? ScaledSize.Y * 0.5f + 18f : 50f);
        for (int i = 0; i < index; i++)
            y -= _expanded ? 40f : CollapsedGap(i);
        float x = _expanded ? Mathf.Max(ScaledSize.X * 0.5f + 16f - index * 12f, ScaledSize.X * 0.5f + 8f) : 16f - index * 8f;
        return new Vector2(viewport.Position.X + x, y);
    }

    private static Vector2 ClipPosition(Vector2 center) => center - ScaledSize * 0.5f - Vector2.One * VisualMargin;

    private static float CollapsedGap(int index) => Mathf.Max(30f - index * 5f, 0f);

    private void Layout(bool animate)
    {
        for (int i = 0; i < _entries.Count; i++)
            MoveChild(_entries[_entries.Count - 1 - i].Clip, i);
        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            float exposedHeight = i == 0 ? ScaledSize.Y : _expanded ? 40f : CollapsedGap(i - 1);
            entry.Clip.Size = new Vector2(ScaledSize.X + VisualMargin * 2f, exposedHeight + VisualMargin * 2f);
            entry.Node.Position = ScaledSize * 0.5f + Vector2.One * VisualMargin;
            float alpha = _expanded ? 1f : i < 4 ? 0.8f - 0.2f * i : 0f;
            entry.Tween?.Kill();
            if (!animate)
            {
                entry.Clip.GlobalPosition = ClipPosition(Center(i));
                entry.Clip.Modulate = new Color(1f, 1f, 1f, alpha);
                continue;
            }
            entry.Tween = entry.Clip.CreateTween().SetParallel();
            entry.Tween.TweenProperty(entry.Clip, "global_position", ClipPosition(Center(i)), LayoutDuration)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
            entry.Tween.TweenProperty(entry.Clip, "modulate:a", alpha, LayoutDuration)
                .SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Cubic);
        }
    }

    private void Subscribe(Entry entry)
    {
        entry.Refresh = () =>
        {
            try
            {
                if (GodotObject.IsInstanceValid(entry.Node) && entry.Node.IsInsideTree())
                    entry.Node.UpdateVisuals(PileType.Draw, CardPreviewMode.Normal);
            }
            catch (Exception exception)
            {
                Disable(exception);
            }
        };
        entry.Model.AfflictionChanged += entry.Refresh;
        entry.Model.EnchantmentChanged += entry.Refresh;
        entry.Model.EnergyCostChanged += entry.Refresh;
        entry.Model.KeywordsChanged += entry.Refresh;
        entry.Model.ReplayCountChanged += entry.Refresh;
        entry.Model.StarCostChanged += entry.Refresh;
        entry.Model.Upgraded += entry.Refresh;
        entry.Model.Forged += entry.Refresh;
    }

    private void Disable(Exception exception)
    {
        _failed = true;
        Clear();
        Hide();
        RitsuLibFramework.CreateLogger(MainFile.ModId).ErrorNoTrace("Nagato draw-pile previews disabled: " + exception.Message);
    }

    private static void Remove(Entry entry)
    {
        entry.Model.AfflictionChanged -= entry.Refresh;
        entry.Model.EnchantmentChanged -= entry.Refresh;
        entry.Model.EnergyCostChanged -= entry.Refresh;
        entry.Model.KeywordsChanged -= entry.Refresh;
        entry.Model.ReplayCountChanged -= entry.Refresh;
        entry.Model.StarCostChanged -= entry.Refresh;
        entry.Model.Upgraded -= entry.Refresh;
        entry.Model.Forged -= entry.Refresh;
        entry.Refresh = null;
        if (GodotObject.IsInstanceValid(entry.Tween))
            entry.Tween!.Kill();
        if (GodotObject.IsInstanceValid(entry.Clip))
            entry.Clip.QueueFree();
    }

    private void Clear()
    {
        _known.Invalidate();
        foreach (var entry in _entries)
            Remove(entry);
        _entries.Clear();
    }

    public override void _ExitTree()
    {
        Clear();
        if (ReferenceEquals(_active, this))
            _active = null;
    }
}
