using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using NagatoSpire2.NagatoSpire2Code.Characters;
using NagatoSpire2.NagatoSpire2Code.Nodes;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

internal static class NagatoSkinSelectPanelController
{
	private const string ScenePath = "res://NagatoSpire2/scenes/ui/nagato_skin_select_panel.tscn";
	private static NagatoSkinSelectPanel? _panelInstance;

	public static void OnCharacterSelected(NCharacterSelectScreen screen, CharacterModel character)
	{
		if (character is not NagatoCharacter nagatoSkin)
		{
			if (GodotObject.IsInstanceValid(_panelInstance))
				_panelInstance.Visible = false;
			return;
		}

		var infoPanel = screen.GetNodeOrNull<Control>("%InfoPanel");
		if (infoPanel is null)
			return;

		if (!GodotObject.IsInstanceValid(_panelInstance) || _panelInstance.GetParent() != infoPanel)
		{
			if (GodotObject.IsInstanceValid(_panelInstance))
				_panelInstance.QueueFreeSafely();

			var scene = ResourceLoader.Load<PackedScene>(ScenePath);
			if (scene is null)
				return;

			_panelInstance = scene.Instantiate<NagatoSkinSelectPanel>(PackedScene.GenEditState.Disabled);
			infoPanel.AddChildSafely(_panelInstance);
			_panelInstance.Position = new Vector2(400f, 0f);
		}

		_panelInstance.SetInteractable(true);
		_panelInstance.ShowAndSync(screen, nagatoSkin);
	}

	public static void SetInteractable(bool interactable)
	{
		if (GodotObject.IsInstanceValid(_panelInstance))
			_panelInstance.SetInteractable(interactable);
	}
}

public sealed class NagatoSkinSelectPatch : IPatchMethod
{
	public static string PatchId => "nagato_skin_select_panel";
	public static string Description => "Show the Nagato Spine skin selector";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(
			typeof(NCharacterSelectScreen),
			nameof(NCharacterSelectScreen.SelectCharacter),
			[typeof(NCharacterSelectButton), typeof(CharacterModel)])
	];

	[HarmonyPostfix]
	public static void Postfix(NCharacterSelectScreen __instance, CharacterModel characterModel)
	{
		NagatoSkinSelectPanelController.OnCharacterSelected(__instance, characterModel);
	}
}

public sealed class NagatoSkinSelectEmbarkPatch : IPatchMethod
{
	public static string PatchId => "nagato_skin_select_panel_embark";
	public static string Description => "Lock the Nagato skin selector while embarking";
	public static bool IsCritical => false;
	public static ModPatchTarget[] GetTargets() => [new(typeof(NCharacterSelectScreen), "OnEmbarkPressed", null)];

	[HarmonyPostfix]
	public static void Postfix() => NagatoSkinSelectPanelController.SetInteractable(false);
}

public sealed class NagatoSkinSelectUnreadyPatch : IPatchMethod
{
	public static string PatchId => "nagato_skin_select_panel_unready";
	public static string Description => "Unlock the Nagato skin selector after unreadying";
	public static bool IsCritical => false;
	public static ModPatchTarget[] GetTargets() => [new(typeof(NCharacterSelectScreen), "OnUnreadyPressed", null)];

	[HarmonyPostfix]
	public static void Postfix() => NagatoSkinSelectPanelController.SetInteractable(true);
}
