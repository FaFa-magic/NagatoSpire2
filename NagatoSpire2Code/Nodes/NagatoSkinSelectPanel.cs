using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using NagatoSpire2.NagatoSpire2Code.Characters;

namespace NagatoSpire2.NagatoSpire2Code.Nodes;

[GlobalClass]
public partial class NagatoSkinSelectPanel : Control
{
	private static readonly NagatoCharacter[] Skins =
	[
		ModelDb.Character<NagatoCharacter>(),
		ModelDb.Character<NagatoVariantTwo>(),
		ModelDb.Character<NagatoVariantThree>(),
		ModelDb.Character<NagatoVariantFour>(),
		ModelDb.Character<NagatoVariantFive>(),
		ModelDb.Character<NagatoVariantSix>(),
		ModelDb.Character<NagatoVariantH>()
	];

	private TextureButton _leftArrow = null!;
	private TextureButton _rightArrow = null!;
	private Control _visualContainer = null!;
	private Label _skinNameLabel = null!;
	private NCharacterSelectScreen _selectScreen = null!;
	private Node2D? _currentVisualNode;
	private ModelId? _renderedSkinId;
	private int _currentIndex;

	public override void _Ready()
	{
		_leftArrow = GetNode<TextureButton>("VBoxContainer/HBoxContainer/LeftArrow");
		_rightArrow = GetNode<TextureButton>("VBoxContainer/HBoxContainer/RightArrow");
		_visualContainer = GetNode<Control>("VBoxContainer/HBoxContainer/VisualContainer");
		_skinNameLabel = GetNode<Label>("VBoxContainer/LabelContainer/SkinNameLabel");

		_leftArrow.Pressed += OnLeftPressed;
		_rightArrow.Pressed += OnRightPressed;
	}

	public void SetInteractable(bool interactable)
	{
		if (GodotObject.IsInstanceValid(_leftArrow))
		{
			_leftArrow.Visible = interactable;
			_leftArrow.Disabled = !interactable;
		}

		if (GodotObject.IsInstanceValid(_rightArrow))
		{
			_rightArrow.Visible = interactable;
			_rightArrow.Disabled = !interactable;
		}
	}

	public void ShowAndSync(NCharacterSelectScreen screen, NagatoCharacter selectedSkin)
	{
		_selectScreen = screen;
		Visible = true;

		var selectedIndex = Array.FindIndex(Skins, skin => skin.Id == selectedSkin.Id);
		_currentIndex = selectedIndex >= 0 ? selectedIndex : 0;

		// SelectCharacter already synchronized the lobby; only refresh the panel here.
		RenderSkinVisuals(Skins[_currentIndex]);
	}

	private void OnLeftPressed()
	{
		_currentIndex = (_currentIndex + Skins.Length - 1) % Skins.Length;
		ApplySelection();
	}

	private void OnRightPressed()
	{
		_currentIndex = (_currentIndex + 1) % Skins.Length;
		ApplySelection();
	}

	private void ApplySelection()
	{
		var skin = Skins[_currentIndex];
		_selectScreen.Lobby.SetLocalCharacter(skin);
		RenderSkinVisuals(skin);
		SfxCmd.Play(skin.CharacterSelectSfx);
	}

	private void RenderSkinVisuals(NagatoCharacter skin)
	{
		_skinNameLabel.Text = new LocString("characters", $"{skin.Id.Entry}.skinName").GetFormattedText();

		if (_renderedSkinId == skin.Id && GodotObject.IsInstanceValid(_currentVisualNode))
			return;

		_renderedSkinId = skin.Id;
		if (GodotObject.IsInstanceValid(_currentVisualNode))
		{
			_visualContainer.RemoveChild(_currentVisualNode);
			_currentVisualNode.QueueFree();
			_currentVisualNode = null;
		}

		var scene = ResourceLoader.Load<PackedScene>(skin.CustomVisualsPath);
		if (scene is null)
			return;

		_currentVisualNode = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
		_visualContainer.AddChild(_currentVisualNode);
		_currentVisualNode.Position = new Vector2(150f, 285f);

		var spineNode = _currentVisualNode.GetNodeOrNull<Node2D>("%Visuals");
		if (!GodotObject.IsInstanceValid(spineNode) || spineNode.GetClass() != MegaSprite.spineClassName)
			return;

		var skeletonData = ResourceLoader.Load<Resource>(skin.CurrentSkinDefinition.SpineSkeletonDataPath);
		if (skeletonData is null)
			return;

		var sprite = new MegaSprite((Variant)(GodotObject)spineNode);
		sprite.SetSkeletonDataRes(new MegaSkeletonDataResource(skeletonData));
		this.RunWhenSpineReady(sprite, animationState => animationState.SetAnimation("normal"));
	}
}
