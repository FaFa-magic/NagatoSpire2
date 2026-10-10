using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoCardDrawnPatch : IPatchMethod
{
	public static string PatchId => "nagato_card_drawn_repeat";
	public static string Description => "Repeat only the drawn card's own draw effects";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(Hook), nameof(Hook.AfterCardDrawn),
			[typeof(ICombatState), typeof(PlayerChoiceContext), typeof(CardModel), typeof(bool)], MethodType.Async)
	];

	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		var earlyHook = AccessTools.Method(typeof(AbstractModel), nameof(AbstractModel.AfterCardDrawnEarly));
		var drawnHook = AccessTools.Method(typeof(AbstractModel), nameof(AbstractModel.AfterCardDrawn));
		var earlyReplacement = AccessTools.Method(typeof(NagatoCardDrawnPatch), nameof(AfterCardDrawnEarly));
		var drawnReplacement = AccessTools.Method(typeof(NagatoCardDrawnPatch), nameof(AfterCardDrawn));
		int earlyCount = 0;
		int drawnCount = 0;

		foreach (var instruction in instructions)
		{
			if (instruction.Calls(earlyHook))
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = earlyReplacement;
				earlyCount++;
			}
			else if (instruction.Calls(drawnHook))
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = drawnReplacement;
				drawnCount++;
			}
			yield return instruction;
		}

		if (earlyCount != 1 || drawnCount != 1)
			throw new InvalidOperationException("Could not locate both card draw hook dispatches.");
	}

	private static Task AfterCardDrawnEarly(AbstractModel model, PlayerChoiceContext choiceContext,
		CardModel card, bool fromHandDraw) => InvokeDrawnHook(model, choiceContext, card, fromHandDraw, true);

	private static Task AfterCardDrawn(AbstractModel model, PlayerChoiceContext choiceContext,
		CardModel card, bool fromHandDraw) => InvokeDrawnHook(model, choiceContext, card, fromHandDraw, false);

	private static async Task InvokeDrawnHook(AbstractModel model, PlayerChoiceContext choiceContext,
		CardModel card, bool fromHandDraw, bool early)
	{
		int repeats = model == card ? Math.Max(0, card.Owner.Creature.GetPower<ClimbMountNiitakaPower>()?.Amount ?? 0) : 0;
		for (int i = 0; i <= repeats; i++)
		{
			if (i > 0)
			{
				if (CombatManager.Instance.IsOverOrEnding || card.Owner.Creature.IsDead)
					break;
				model.InvokeExecutionFinished();
			}

			if (early)
				await model.AfterCardDrawnEarly(choiceContext, card, fromHandDraw);
			else
				await model.AfterCardDrawn(choiceContext, card, fromHandDraw);
		}
	}
}
