using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoSealedAutoplayPatch : IPatchMethod
{
	public static string PatchId => "nagato_sealed_autoplay";
	public static string Description => "Allow drawn sealed cards through the native unplayable autoplay check";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CardCmd), nameof(CardCmd.AutoPlay),
			[typeof(PlayerChoiceContext), typeof(CardModel), typeof(Creature), typeof(AutoPlayType), typeof(bool), typeof(bool)], MethodType.Async)
	];

	[HarmonyTranspiler]
	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
	{
		var getter = AccessTools.PropertyGetter(typeof(CardModel), nameof(CardModel.Keywords));
		var replacement = AccessTools.Method(typeof(NagatoSealedAutoplayPatch), nameof(GetAutoplayKeywords));
		int count = 0;
		foreach (var instruction in instructions)
		{
			if (instruction.Calls(getter))
			{
				instruction.opcode = OpCodes.Call;
				instruction.operand = replacement;
				count++;
			}
			yield return instruction;
		}
		if (count != 1)
			throw new InvalidOperationException("Could not locate the native autoplay keyword check.");
	}

	private static IReadOnlySet<CardKeyword> GetAutoplayKeywords(CardModel card) =>
		card.Affliction is Sealed { IsAutoplaying: true }
			? card.Keywords.Where(keyword => keyword != CardKeyword.Unplayable).ToHashSet()
			: card.Keywords;
}
