using System.Threading;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

/// <summary>
/// Extend only Nagato's character-select material timeline. The original
/// NTransition retains Instant mode, cancellation, input blocking and loading.
/// </summary>
public sealed class NagatoTransitionPatch : IPatchMethod
{
	public const string TransitionPath = "res://NagatoSpire2/materials/nagato_transition_mat.tres";
	public const float MinimumDuration = 5.4f;

	public static string PatchId => "nagato_sakura_character_transition";
	public static string Description => "Play Nagato's 5.4-second ink, shrine-water and sakura transition";
	public static bool IsCritical => false;
	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NTransition), nameof(NTransition.FadeOut),
			[typeof(float), typeof(string), typeof(CancellationToken?)])
	];

	[HarmonyPrefix]
	public static void Prefix(NTransition __instance, ref float time, string transitionPath,
		out HiddenOverlays? __state)
	{
		__state = null;
		if (transitionPath != TransitionPath || !float.IsFinite(time) || time <= 0f)
			return;

		time = Math.Max(time, MinimumDuration);
		// The stock parallel SimpleTransition would darken the illustration while
		// our shader is still playing. Hide only its children for this one call;
		// restore their original visibility even if the async task fails/cancels.
		__state = new HiddenOverlays(__instance.GetNodeOrNull<Control>("SimpleTransition"),
			__instance.GetNodeOrNull<Control>("GradientTransition"));
	}

	[HarmonyPostfix]
	public static void Postfix(ref Task __result, HiddenOverlays? __state)
	{
		if (__state is not null)
			__result = RestoreAfterTransition(__result, __state);
	}

	private static async Task RestoreAfterTransition(Task transition, HiddenOverlays overlays)
	{
		try { await transition; }
		finally { overlays.Restore(); }
	}

	public sealed class HiddenOverlays
	{
		private readonly Control? _simple;
		private readonly Control? _gradient;
		private readonly bool _simpleVisible;
		private readonly bool _gradientVisible;

		public HiddenOverlays(Control? simple, Control? gradient)
		{
			_simple = simple;
			_gradient = gradient;
			if (GodotObject.IsInstanceValid(simple))
			{
				_simpleVisible = simple!.Visible;
				simple.Visible = false;
			}
			if (GodotObject.IsInstanceValid(gradient))
			{
				_gradientVisible = gradient!.Visible;
				gradient.Visible = false;
			}
		}

		public void Restore()
		{
			if (GodotObject.IsInstanceValid(_simple))
				_simple!.Visible = _simpleVisible;
			if (GodotObject.IsInstanceValid(_gradient))
				_gradient!.Visible = _gradientVisible;
		}
	}
}
