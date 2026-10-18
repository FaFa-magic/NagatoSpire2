using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib;
using STS2RitsuLib.Combat.HandSize;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoOverflowDrawPatch : IPatchMethod
{
	public static string PatchId => "nagato_overflow_draw";
	public static string Description => "Keep native draw processing while sending Still Waters overflow draws to the discard pile";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardPileCmd), "DrawInternal", [typeof(PlayerChoiceContext), typeof(decimal), typeof(Player), typeof(bool)], MethodType.Async),
		new(typeof(CardPileCmd), "CheckIfDrawIsPossibleAndShowThoughtBubbleIfNot", [typeof(Player)]),
		new(typeof(CombatManager), "SetupPlayerTurn", MethodType.Async)
	];

	[HarmonyTranspiler]
	[HarmonyPriority(Priority.Last)]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase __originalMethod)
	{
		var handLimit = AccessTools.Method(typeof(MaxHandSizeCalculator), nameof(MaxHandSizeCalculator.Calculate));
		var drawLimit = AccessTools.Method(typeof(NagatoOverflowDrawPatch), nameof(GetDrawLimit));
		var add = AccessTools.Method(typeof(CardPileCmd), nameof(CardPileCmd.Add),
			[typeof(CardModel), typeof(CardPile), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool)]);
		var addDrawn = AccessTools.Method(typeof(NagatoOverflowDrawPatch), nameof(AddDrawnCard));
		int limits = 0;
		int additions = 0;
		foreach (var instruction in instructions)
		{
			if (instruction.Calls(handLimit))
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = drawLimit;
				limits++;
			}
			else if (instruction.Calls(add))
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = addDrawn;
				additions++;
			}
			yield return instruction;
		}
		if (limits == 0 || __originalMethod.DeclaringType?.Name.Contains("<DrawInternal>") == true && additions != 1)
			throw new InvalidOperationException("Could not locate native draw limits or the drawn-card pile transition.");
	}

	private static int GetDrawLimit(Player player) => player.Creature.GetPowerAmount<StillWatersPower>() > 0
		? int.MaxValue : RitsuLibFramework.GetMaxHandSize(player);

	private static Task<CardPileAddResult> AddDrawnCard(CardModel card, CardPile pile, CardPilePosition position,
		AbstractModel? clonedBy, bool skipVisuals)
	{
		if (pile.Type == PileType.Hand && card.Owner.Creature.GetPowerAmount<StillWatersPower>() > 0 &&
			pile.Cards.Count >= RitsuLibFramework.GetMaxHandSize(card.Owner))
			pile = PileType.Discard.GetPile(card.Owner);
		return CardPileCmd.Add(card, pile, position, clonedBy, skipVisuals);
	}
}
