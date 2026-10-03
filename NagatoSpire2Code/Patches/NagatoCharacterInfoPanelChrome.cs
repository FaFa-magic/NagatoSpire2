using System.Runtime.CompilerServices;
using Godot;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

/// <summary>Replace only Nagato's character info backdrop, leaving the official content intact.</summary>
internal static class NagatoCharacterInfoPanelChrome
{
	private const string BackgroundScenePath = "res://NagatoSpire2/scenes/ui/nagato_skin_name_background.tscn";
	private static readonly ConditionalWeakTable<Control, BackgroundState> States = new();

	public static void Update(Control infoPanel, bool isNagato)
	{
		if (!isNagato)
		{
			if (States.TryGetValue(infoPanel, out var existing))
				existing.Restore();
			return;
		}

		var original = infoPanel.GetNodeOrNull<Control>("NinePatchRect");
		if (original is null)
			return;

		States.GetValue(infoPanel, panel => new BackgroundState(panel, original)).Apply();
	}

	private sealed class BackgroundState(Control infoPanel, Control original)
	{
		private Control? _background;
		private bool _active;
		private bool _originalVisible;

		public void Apply()
		{
			if (!GodotObject.IsInstanceValid(_background))
			{
				var scene = ResourceLoader.Load<PackedScene>(BackgroundScenePath);
				if (scene is null)
					return;

				_background = scene.Instantiate<Control>();
				_background.Name = "NagatoInfoPanelBackground";
				infoPanel.AddChild(_background);
				// Keep the replacement behind the official text, relic and skin controls.
				infoPanel.MoveChild(_background, original.GetIndex() + 1);
				original.Resized += SyncLayout;
			}

			if (!_active)
				_originalVisible = original.Visible;
			SyncLayout();
			_background!.Visible = true;
			original.Visible = false;
			_active = true;
		}

		public void Restore()
		{
			if (!_active)
				return;
			if (GodotObject.IsInstanceValid(_background))
				_background!.Visible = false;
			if (GodotObject.IsInstanceValid(original))
				original.Visible = _originalVisible;
			_active = false;
		}

		private void SyncLayout()
		{
			if (!GodotObject.IsInstanceValid(_background) || !GodotObject.IsInstanceValid(original))
				return;

			// The official scene uses full-rect anchors and (-87, -24, 87, -56)
			// offsets. Copy the actual node rather than hard-coding a screen size.
			_background!.AnchorLeft = original.AnchorLeft;
			_background.AnchorTop = original.AnchorTop;
			_background.AnchorRight = original.AnchorRight;
			_background.AnchorBottom = original.AnchorBottom;
			_background.OffsetLeft = original.OffsetLeft;
			_background.OffsetTop = original.OffsetTop;
			_background.OffsetRight = original.OffsetRight;
			_background.OffsetBottom = original.OffsetBottom;
			_background.GrowHorizontal = original.GrowHorizontal;
			_background.GrowVertical = original.GrowVertical;
		}
	}
}
