using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using NagatoSpire2.NagatoSpire2Code.Characters;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

/// <summary>
/// Restarts Nagato's idle animation after RitsuLib replaces the combat Spine skeleton.
/// </summary>
public sealed class NagatoCombatSpineIdleBootstrapPatch : IPatchMethod
{
	public static string PatchId => "nagato_combat_spine_idle_bootstrap";

	public static string Description =>
		"Restart Nagato's combat idle animation after the selected Spine skeleton is ready";

	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(NCreature), nameof(NCreature._Ready))];

	[HarmonyPostfix]
	[HarmonyPriority(Priority.Last)]
	public static void Postfix(NCreature __instance)
	{
		if (__instance.Entity?.Player?.Character is not NagatoCharacter ||
		    __instance.Visuals?.SpineBody is not { } spineBody)
		{
			return;
		}

		// RitsuLib applies CombatSkeletonDataPath from an NCreature._Ready postfix.
		// That rebuilds the animation state and clears the idle track created earlier.
		Callable.From(() =>
		{
			if (!GodotObject.IsInstanceValid(__instance) ||
			    !__instance.IsInsideTree() ||
			    __instance.Entity.IsDead ||
			    __instance.Visuals?.SpineBody != spineBody)
			{
				return;
			}

			__instance.RunWhenSpineReady(spineBody, _ =>
			{
				if (GodotObject.IsInstanceValid(__instance) &&
				    __instance.IsInsideTree() &&
				    !__instance.Entity.IsDead)
				{
					__instance.SetAnimationTrigger(CreatureAnimator.idleTrigger);
				}
			});
		}).CallDeferred();
	}
}
