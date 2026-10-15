using Godot;
using HarmonyLib;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Orbs;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using NagatoSpire2.NagatoSpire2Code.Cards;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public static class NagatoOrbTargetingPatches
{
	private sealed record OrbBinding(NOrb Orb, Control Bounds, NSelectionReticle Reticle, Action Enter, Action Exit);
	private static readonly ConditionalWeakTable<NMouseCardPlay, object> SelectedMousePlays = new();

	private static readonly AccessTools.FieldRef<NOrbManager, List<NOrb>> OrbNodes =
		AccessTools.FieldRefAccess<NOrbManager, List<NOrb>>("_orbs");

	private static readonly Action<NCardPlay> TryShowEvokingOrbs =
		AccessTools.MethodDelegate<Action<NCardPlay>>(
			AccessTools.DeclaredMethod(typeof(NCardPlay), "TryShowEvokingOrbs"));

	private static readonly Action<NCardPlay> CenterCard =
		AccessTools.MethodDelegate<Action<NCardPlay>>(
			AccessTools.DeclaredMethod(typeof(NCardPlay), "CenterCard"));

	private static readonly Action<NCardPlay, Creature?> TryPlayCard =
		AccessTools.MethodDelegate<Action<NCardPlay, Creature?>>(
			AccessTools.DeclaredMethod(typeof(NCardPlay), "TryPlayCard", [typeof(Creature)]));

	public static NOrb[] GetOrbs(NagatoOrbSlotCardModel card)
	{
		if (card.Owner.PlayerCombatState is not { } state)
			return [];

		var manager = NCombatRoom.Instance?.GetCreatureNode(card.Owner.Creature)?.OrbManager;
		if (manager == null)
			return [];

		return OrbNodes(manager).Take(state.OrbQueue.Capacity).ToArray();
	}

	public static int GetSlotIndex(NagatoOrbSlotCardModel card, NOrb orb) => Array.IndexOf(GetOrbs(card), orb);

	public static async Task<NOrb?> SelectOrb(NCardPlay play, TargetMode mode, NOrb[] orbs, Func<bool> shouldExit)
	{
		NTargetManager manager = NTargetManager.Instance;
		NCombatRoom? room = NCombatRoom.Instance;
		if (room == null)
			return null;
		var allowed = orbs.ToHashSet();
		var bindings = new List<OrbBinding>();
		var navigation = new List<(NOrb Orb, NodePath Left, NodePath Right, NodePath Top, NodePath Bottom)>();
		bool controller = mode == TargetMode.Controller;
		bool selectionFinished = false;
		try
		{
			foreach (NOrb orb in orbs)
			{
				Control bounds = orb.GetNode<Control>("Bounds");
				NSelectionReticle reticle = orb.GetNode<NSelectionReticle>("%SelectionReticle");
				Action enter = () =>
				{
					if (!manager.IsInSelection || !manager.AllowedToTargetNode(orb))
						return;
					manager.OnNodeHovered(orb);
					reticle.OnSelect();
				};
				Action exit = () =>
				{
					manager.OnNodeUnhovered(orb);
					reticle.OnDeselect();
				};
				if (controller)
				{
					orb.FocusEntered += enter;
					orb.FocusExited += exit;
				}
				else
				{
					bounds.MouseEntered += enter;
					bounds.MouseExited += exit;
				}
				bindings.Add(new OrbBinding(orb, bounds, reticle, enter, exit));
			}

			manager.StartTargeting(TargetType.TargetedNoCreature, play.Holder.CardNode!, mode, shouldExit,
				node => node is NOrb orb && allowed.Contains(orb) &&
				play.Holder.CardModel is NagatoOrbSlotCardModel card && GetSlotIndex(card, orb) >= 0);

			if (controller)
			{
				var ownerNode = room.GetCreatureNode(play.Player.Creature);
				if (ownerNode == null)
					return null;
				room.RestrictControllerNavigation([ownerNode.Hitbox]);
				ownerNode.Hitbox.FocusMode = Control.FocusModeEnum.None;
				for (int i = 0; i < orbs.Length; i++)
				{
					NOrb orb = orbs[i];
					navigation.Add((orb, orb.FocusNeighborLeft, orb.FocusNeighborRight, orb.FocusNeighborTop, orb.FocusNeighborBottom));
					orb.FocusNeighborLeft = orbs[(i + 1) % orbs.Length].GetPath();
					orb.FocusNeighborRight = orbs[(i + orbs.Length - 1) % orbs.Length].GetPath();
					orb.FocusNeighborTop = orb.GetPath();
					orb.FocusNeighborBottom = orb.GetPath();
				}
				orbs[0].GrabFocus();
			}
			else
			{
				Vector2 mouse = play.GetViewport().GetMousePosition();
				foreach (OrbBinding binding in bindings)
				{
					if (binding.Bounds.GetGlobalRect().HasPoint(mouse))
					{
						binding.Enter();
						break;
					}
				}
			}

			NOrb? selected = await manager.SelectionFinished() as NOrb;
			selectionFinished = true;
			return selected;
		}
		finally
		{
			if (!selectionFinished && manager.IsInSelection)
				manager.CancelTargeting();
			foreach (OrbBinding binding in bindings)
			{
				if (GodotObject.IsInstanceValid(binding.Reticle))
					binding.Reticle.OnDeselect();
				if (!GodotObject.IsInstanceValid(binding.Orb) || !GodotObject.IsInstanceValid(binding.Bounds))
					continue;
				if (controller)
				{
					binding.Orb.FocusEntered -= binding.Enter;
					binding.Orb.FocusExited -= binding.Exit;
				}
				else
				{
					binding.Bounds.MouseEntered -= binding.Enter;
					binding.Bounds.MouseExited -= binding.Exit;
				}
			}
			foreach (var item in navigation)
			{
				if (!GodotObject.IsInstanceValid(item.Orb))
					continue;
				item.Orb.FocusNeighborLeft = item.Left;
				item.Orb.FocusNeighborRight = item.Right;
				item.Orb.FocusNeighborTop = item.Top;
				item.Orb.FocusNeighborBottom = item.Bottom;
			}
			if (controller && GodotObject.IsInstanceValid(room))
				room.EnableControllerNavigation();
		}
	}

	public sealed class MousePatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_mouse_targeting";
		public static string Description => "Target orb slots while dragging orb-slot cards";
		public static bool IsCritical => true;

		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(NMouseCardPlay), "TargetSelection", [typeof(TargetMode)])
		];

		[HarmonyPrefix]
		public static bool Prefix(NMouseCardPlay __instance, TargetMode targetMode, ref Task __result)
		{
			if (__instance.Holder.CardModel is not NagatoOrbSlotCardModel card)
				return true;

			__result = Run(__instance, card, targetMode);
			return false;
		}

		private static async Task Run(NMouseCardPlay play, NagatoOrbSlotCardModel card, TargetMode mode)
		{
			card.SetOrbSlotTarget(null);
			TryShowEvokingOrbs(play);
			play.Holder.CardNode?.CardHighlight.AnimFlash();
			NOrb[] orbs = GetOrbs(card);
			if (orbs.Length == 0)
			{
				SelectedMousePlays.GetValue(play, _ => new object());
				return;
			}

			CenterCard(play);
			NOrb? selected = await SelectOrb(play, mode, orbs,
				() => !GodotObject.IsInstanceValid(play) || !play.IsInsideTree());
			if (!GodotObject.IsInstanceValid(play))
				return;
			if (selected != null && GetSlotIndex(card, selected) is var index && index >= 0)
			{
				card.SetOrbSlotTarget(index);
				SelectedMousePlays.GetValue(play, _ => new object());
			}
			else if (!Input.IsMouseButtonPressed(MouseButton.Right) &&
				!Input.IsActionPressed(MegaInput.cancel) &&
				!Input.IsActionPressed(MegaInput.pauseAndBack) &&
				!Input.IsActionPressed(MegaInput.topPanel))
				SelectedMousePlays.GetValue(play, _ => new object());
			else
				play.CancelPlayCard();
		}
	}

	public sealed class MousePlayZonePatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_mouse_play_zone";
		public static string Description => "Allow orb-slot cards to resolve below the normal play zone";
		public static bool IsCritical => true;

		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(NMouseCardPlay), "IsCardInPlayZone", Type.EmptyTypes)
		];

		[HarmonyPostfix]
		public static void Postfix(NMouseCardPlay __instance, ref bool __result)
		{
			if (SelectedMousePlays.TryGetValue(__instance, out _))
				__result = true;
		}
	}

	public sealed class ControllerPatch : IPatchMethod
	{
		public static string PatchId => "nagato_orb_controller_targeting";
		public static string Description => "Target orb slots with controller card play";
		public static bool IsCritical => true;

		public static ModPatchTarget[] GetTargets() =>
		[
			new(typeof(NControllerCardPlay), nameof(NControllerCardPlay.Start), Type.EmptyTypes)
		];

		[HarmonyPrefix]
		public static bool Prefix(NControllerCardPlay __instance)
		{
			if (__instance.Holder.CardModel is not NagatoOrbSlotCardModel card)
				return true;

			TaskHelper.RunSafely(Run(__instance, card));
			return false;
		}

		private static async Task Run(NControllerCardPlay play, NagatoOrbSlotCardModel card)
		{
			card.SetOrbSlotTarget(null);
			if (play.Holder.CardNode == null)
				return;

			NDebugAudioManager.Instance?.Play("card_select.mp3");
			NHoverTipSet.Remove(play.Holder);
			if (!card.CanPlay())
			{
				play.CancelPlayCard();
				return;
			}

			TryShowEvokingOrbs(play);
			play.Holder.CardNode.CardHighlight.AnimFlash();
			CenterCard(play);
			NOrb[] orbs = GetOrbs(card);
			if (orbs.Length == 0)
			{
				TryPlayCard(play, null);
				return;
			}

			NOrb? selected = await SelectOrb(play, TargetMode.Controller, orbs,
				() => !GodotObject.IsInstanceValid(play) || !play.IsInsideTree());
			if (!GodotObject.IsInstanceValid(play))
				return;
			if (selected != null && GetSlotIndex(card, selected) is var index && index >= 0)
			{
				card.SetOrbSlotTarget(index);
				TryPlayCard(play, null);
			}
			else
				play.CancelPlayCard();
		}
	}
}
