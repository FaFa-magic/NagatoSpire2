using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.TestSupport;

namespace NagatoSpire2.NagatoSpire2Code.Nodes;

public sealed class SakuraFormBackground
{
	private const string ScenePath = "res://NagatoSpire2/scenes/vfx/sakura_form_background.tscn";
	private readonly TextureRect _overlay;
	private readonly ShaderMaterial _material;
	private Tween? _tween;

	private SakuraFormBackground(TextureRect overlay, ShaderMaterial material)
	{
		_overlay = overlay;
		_material = material;
	}

	public static SakuraFormBackground? Create()
	{
		if (TestMode.IsOn || NCombatRoom.Instance is not { } room)
			return null;

		TextureRect overlay = PreloadManager.Cache.GetScene(ScenePath)
			.Instantiate<TextureRect>(PackedScene.GenEditState.Disabled);
		ShaderMaterial material = (ShaderMaterial)overlay.Material.Duplicate();
		overlay.Material = material;
		material.SetShaderParameter("reveal", 0f);
		room.SceneContainer.AddChild(overlay);
		int foregroundIndex = room.SceneContainer.GetNode("BackCombatVfxContainer").GetIndex();
		room.SceneContainer.MoveChild(overlay, foregroundIndex);

		SakuraFormBackground background = new(overlay, material);
		background._tween = overlay.CreateTween();
		background._tween.TweenProperty(material, "shader_parameter/reveal", 1f, 2.6f)
			.SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Sine);
		return background;
	}

	public void FadeOut()
	{
		if (!GodotObject.IsInstanceValid(_overlay))
			return;

		if (_tween is not null && GodotObject.IsInstanceValid(_tween))
			_tween.Kill();
		_tween = _overlay.CreateTween();
		_tween.TweenProperty(_material, "shader_parameter/reveal", 0f, 1.8f)
			.SetEase(Tween.EaseType.InOut)
			.SetTrans(Tween.TransitionType.Sine);
		_tween.TweenCallback(Callable.From(_overlay.QueueFree));
	}
}
