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
	private OrbModel? _model;
	private NOrb? _orb;
	private Node2D? _preview;
	private bool _inCombat;

	public static void Refresh(NCard card)
	{
		if (!card.IsNodeReady())
			return;
		var view = card.Body.GetNodeOrNull<NNagatoCardOrb>("NagatoCardOrb");
		if (view == null && card.Model != null && NagatoCardOrbs.Get(card.Model) != null)
		{
			Vector2 energyCenter = card.GetNode<Control>("%EnergyIcon").GetRect().GetCenter();
			view = new NNagatoCardOrb
			{
				Name = "NagatoCardOrb", _card = card, MouseFilter = MouseFilterEnum.Ignore,
				Position = new Vector2(-energyCenter.X, energyCenter.Y)
			};
			card.Body.AddChild(view);
		}
		view?.RefreshOrb();
	}

	public override void _Ready()
	{
		var socket = new Panel
		{
			Name = "Socket", Position = new Vector2(-38f, -30f), Size = new Vector2(76f, 76f),
			MouseFilter = MouseFilterEnum.Ignore
		};
		socket.AddThemeStyleboxOverride("panel", new StyleBoxFlat
		{
			BgColor = new Color("#291c24"), BorderColor = new Color("#b79a57"),
			BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2,
			CornerRadiusTopLeft = 38, CornerRadiusTopRight = 38, CornerRadiusBottomLeft = 38, CornerRadiusBottomRight = 38,
			ShadowColor = new Color(0.07f, 0.035f, 0.045f, 0.6f), ShadowSize = 3
		});
		AddChild(socket);
		MoveChild(socket, 0);
	}

	public override void _Process(double delta) => RefreshOrb();

	private void RefreshOrb()
	{
		var model = _card.Model == null ? null : NagatoCardOrbs.Get(_card.Model);
		bool inCombat = CombatManager.Instance.IsInProgress && _card.Model?.Pile?.IsCombatPile == true;
		if (model != _model || inCombat != _inCombat)
		{
			_orb?.QueueFree();
			_orb = null;
			_preview?.QueueFree();
			_preview = null;
			_model = model;
			_inCombat = inCombat;
			if (model != null)
			{
				Vector2 center = GetNode<Control>("Socket").GetRect().GetCenter();
				if (!inCombat)
				{
					_preview = model.CreateSprite();
					AddChild(_preview);
					_preview.Position = center;
					_preview.Scale = Vector2.One * 1.8f;
					if (_preview is NOrbVfx vfx)
						vfx.Initialize(model);
					if (_preview.GetNodeOrNull("SpineSkeleton") is { } spine)
						this.RunWhenSpineReady(new MegaSprite(spine), state => state.SetAnimation("idle_loop"));
				}
				else
				{
					_orb = NOrb.Create(true, model);
					AddChild(_orb);
					_orb.SetAnchorsAndOffsetsPreset(LayoutPreset.TopLeft);
					_orb.Position = center;
					_orb.Scale = Vector2.One * 1.2f;
					_orb.FocusMode = FocusModeEnum.None;
					_orb.MouseFilter = MouseFilterEnum.Ignore;
					foreach (var control in _orb.FindChildren("*", "Control", true, false).OfType<Control>())
						control.MouseFilter = MouseFilterEnum.Ignore;
				}
			}
		}
		Visible = _model != null;
		_orb?.UpdateVisuals(false);
	}
}
