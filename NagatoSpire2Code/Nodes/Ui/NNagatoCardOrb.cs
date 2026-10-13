using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using NagatoSpire2.NagatoSpire2Code.Combat;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

public partial class NNagatoCardOrb : Control
{
	private NCard _card = null!;
	private OrbModel? _model;
	private NOrb? _orb;

	public static void Refresh(NCard card)
	{
		if (!card.IsNodeReady())
			return;
		var view = card.Body.GetNodeOrNull<NNagatoCardOrb>("NagatoCardOrb");
		if (view == null && card.Model != null && NagatoOrbBindings.GetAttachedOrb(card.Model) != null)
		{
			view = new NNagatoCardOrb
			{
				Name = "NagatoCardOrb", _card = card, MouseFilter = MouseFilterEnum.Ignore,
				Position = new Vector2(112f, -166f), ZIndex = 10
			};
			card.Body.AddChild(view);
		}
		view?.RefreshOrb();
	}

	public override void _Process(double delta) => RefreshOrb();

	private void RefreshOrb()
	{
		var model = _card.Model == null ? null : NagatoOrbBindings.GetAttachedOrb(_card.Model);
		if (model != _model)
		{
			_orb?.QueueFree();
			_orb = null;
			_model = model;
			if (model != null)
			{
				_orb = NOrb.Create(true, model);
				AddChild(_orb);
				_orb.SetAnchorsAndOffsetsPreset(LayoutPreset.TopLeft);
				_orb.Position = Vector2.Zero;
				_orb.Scale = Vector2.One * 0.4f;
				_orb.FocusMode = FocusModeEnum.None;
				_orb.MouseFilter = MouseFilterEnum.Ignore;
				foreach (var control in _orb.FindChildren("*", "Control", true, false).OfType<Control>())
					control.MouseFilter = MouseFilterEnum.Ignore;
			}
		}
		Visible = _model != null;
		_orb?.UpdateVisuals(false);
	}
}
