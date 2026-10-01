using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using NagatoSpire2.NagatoSpire2Code.Characters;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

/// <summary>
/// Replaces the game's round empty-orb outline only for Nagato's orb slots.
/// NOrb hides Outline while a projectile occupies the slot, so this affects
/// empty slots without layering a second frame over the shell illustration.
/// </summary>
public sealed class NagatoEmptyOrbFramePatch : IPatchMethod
{
	private const string FramePath = "res://NagatoSpire2/images/orbs/nagato_empty_shell_frame.png";
	private static readonly FieldInfo? CreatureNodeField =
		AccessTools.Field(typeof(NOrbManager), "_creatureNode");
	private static Texture2D? _frameTexture;

	public static string PatchId => "nagato_empty_shell_orb_frame";
	public static string Description => "Show a Nagato shell-shaped frame in empty orb slots";
	public static bool IsCritical => false;

	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(NOrb), nameof(NOrb._Ready))];

	[HarmonyPostfix]
	public static void Postfix(NOrb __instance)
	{
		if (__instance.GetParent()?.GetParent() is not NOrbManager manager ||
			CreatureNodeField?.GetValue(manager) is not NCreature creature ||
			creature.Entity?.Player?.Character is not NagatoCharacter)
			return;

		_frameTexture ??= ResourceLoader.Load<Texture2D>(FramePath);
		if (_frameTexture is null)
			return;

		if (__instance.GetNodeOrNull<TextureRect>("%Outline") is { } outline)
			outline.Texture = _frameTexture;
		if (__instance.GetNodeOrNull<CpuParticles2D>("Flash") is { } flash)
			flash.Texture = _frameTexture;
	}
}
