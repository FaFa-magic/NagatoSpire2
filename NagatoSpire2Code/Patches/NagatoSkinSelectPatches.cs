using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.TestSupport;
using NagatoSpire2.NagatoSpire2Code.Audio;
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
	private static NCharacterSelectScreen? _audioOwnerScreen;

	public static void OnCharacterSelected(NCharacterSelectScreen screen, CharacterModel character)
	{
		if (!ReferenceEquals(_audioOwnerScreen, screen))
		{
			_audioOwnerScreen = screen;
			// RitsuLib 0.6.2 Screen scope requires explicit cleanup.
			screen.TreeExiting += () =>
			{
				if (!ReferenceEquals(_audioOwnerScreen, screen))
					return;
				NagatoAudio.StopCharacterSelectVoice();
				_audioOwnerScreen = null;
			};
		}
		var infoPanel = screen.GetNodeOrNull<Control>("%InfoPanel");
		if (infoPanel is not null)
			NagatoCharacterInfoPanelChrome.Update(infoPanel, character is NagatoCharacter);

		if (character is not NagatoCharacter nagatoSkin)
		{
			if (GodotObject.IsInstanceValid(_panelInstance))
				_panelInstance.Visible = false;
			return;
		}

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
	[HarmonyPrefix]
	public static void Prefix() => NagatoAudio.StopCharacterSelectVoice();

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

public sealed class NagatoCharacterSelectVoicePatch : IPatchMethod
{
	public static string PatchId => "nagato_character_select_voice_lifecycle";
	public static string Description => "Keep Nagato's character-select voice stoppable across selection and transition";
	public static bool IsCritical => false;
	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NAudioManager), nameof(NAudioManager.PlayOneShot),
			[typeof(string), typeof(Dictionary<string, float>), typeof(float)])
	];

	[HarmonyPrefix]
	[HarmonyPriority(Priority.First)]
	public static bool Prefix(string path, float volume)
	{
		if (path == NagatoAudio.CharacterTransitionEvent)
			NagatoAudio.StopCharacterSelectVoice();
		if (path != NagatoAudio.CharacterSelectEvent || TestMode.IsOn)
			return true;
		NagatoAudio.PlayCharacterSelectVoice(volume);
		return false;
	}
}

public sealed class NagatoSkinSelectEmbarkPatch : IPatchMethod
{
	public static string PatchId => "nagato_skin_select_panel_embark";
	public static string Description => "Lock the Nagato skin selector while embarking";
	public static bool IsCritical => false;
	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NCharacterSelectScreen), "OnEmbarkPressed", null),
		new(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuClosed), [])
	];

	[HarmonyPrefix]
	public static void Prefix() => NagatoAudio.StopCharacterSelectVoice();

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
