using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Helpers;
using NagatoSpire2.NagatoSpire2Code.Combat;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

public partial class NNagatoCardOrb : Control
{
	private NCard _card = null!;
	private readonly List<OrbView> _views = [];

	public static void Refresh(NCard card)
	{
		if (!card.IsNodeReady())
			return;
		var view = card.Body.GetNodeOrNull<NNagatoCardOrb>("NagatoCardOrb");
		if (view == null && card.Model != null && NagatoCardOrbs.Get(card.Model).Count > 0)
		{
			Vector2 energyCenter = card.GetNode<Control>("%EnergyIcon").GetRect().GetCenter();
			view = new NNagatoCardOrb
			{
				Name = "NagatoCardOrb", _card = card, MouseFilter = MouseFilterEnum.Ignore,
				Position = new Vector2(-energyCenter.X, energyCenter.Y)
			};
			card.Body.AddChild(view);
		}
		view?.RefreshOrbs();
	}

	public override void _Process(double delta) => RefreshOrbs();

	private void RefreshOrbs()
	{
		var models = _card.Model == null ? [] : NagatoCardOrbs.Get(_card.Model);
		bool inCombat = CombatManager.Instance.IsInProgress && _card.Model?.Pile?.IsCombatPile == true;
		while (_views.Count > models.Count)
		{
			_views[^1].Node.QueueFree();
			_views.RemoveAt(_views.Count - 1);
		}
		for (int i = 0; i < models.Count; i++)
		{
			if (i == _views.Count)
			{
				var view = new OrbView(i);
				_views.Add(view);
				AddChild(view.Node);
			}
			_views[i].Refresh(models[i], inCombat);
		}
		Visible = models.Count > 0;
	}

	private sealed class OrbView
	{
		private const float OrbScale = 0.75f;
		private const float SocketDiameter = 76f;
		private const float SlotSpacing = SocketDiameter * OrbScale * 1.25f;

		public Control Node { get; }
		private OrbModel? _model;
		private NOrb? _orb;
		private Node2D? _preview;
		private bool _inCombat;

		public OrbView(int index)
		{
			Node = new Control
			{
				Name = $"OrbSlot{index + 1}", Position = new Vector2(0f, 8f + index * SlotSpacing),
				Scale = Vector2.One * OrbScale, MouseFilter = MouseFilterEnum.Ignore
			};
			var socket = new Panel
			{
				Name = "Socket", Position = Vector2.One * (-SocketDiameter * 0.5f), Size = Vector2.One * SocketDiameter,
				MouseFilter = MouseFilterEnum.Ignore
			};
			socket.AddThemeStyleboxOverride("panel", new StyleBoxFlat
			{
				BgColor = new Color("#291c24"), BorderColor = new Color("#b79a57"),
				BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2,
				CornerRadiusTopLeft = 38, CornerRadiusTopRight = 38, CornerRadiusBottomLeft = 38, CornerRadiusBottomRight = 38,
				ShadowColor = new Color(0.07f, 0.035f, 0.045f, 0.6f), ShadowSize = 3
			});
			Node.AddChild(socket);
		}

		public void Refresh(OrbModel model, bool inCombat)
		{
			if (model != _model || inCombat != _inCombat)
			{
				_orb?.QueueFree();
				_orb = null;
				_preview?.QueueFree();
				_preview = null;
				_model = model;
				_inCombat = inCombat;
				if (!inCombat)
				{
					_preview = model.CreateSprite();
					Node.AddChild(_preview);
					_preview.Scale = Vector2.One * 1.8f;
					if (_preview is NOrbVfx vfx)
						vfx.Initialize(model);
					if (_preview.GetNodeOrNull("SpineSkeleton") is { } spine)
						_preview.RunWhenSpineReady(new MegaSprite(spine), state => state.SetAnimation("idle_loop"));
				}
				else
				{
					_orb = NOrb.Create(true, model);
					Node.AddChild(_orb);
					_orb.SetAnchorsAndOffsetsPreset(LayoutPreset.TopLeft);
					_orb.Scale = Vector2.One * 1.2f;
					_orb.FocusMode = FocusModeEnum.None;
					_orb.MouseFilter = MouseFilterEnum.Ignore;
					foreach (var control in _orb.FindChildren("*", "Control", true, false).OfType<Control>())
						control.MouseFilter = MouseFilterEnum.Ignore;
				}
			}
			_orb?.UpdateVisuals(false);
		}
	}
}
