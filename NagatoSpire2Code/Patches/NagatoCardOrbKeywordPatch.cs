using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoCardOrbKeywordPatch : IPatchMethod
{
	public static string PatchId => "nagato_card_orb_keywords";
	public static string Description => "Grant resident Mountain Retain without changing a card's local keywords";
	public static bool IsCritical => true;
	public static ModPatchTarget[] GetTargets() =>
		[new(typeof(CardModel), nameof(CardModel.GetKeywordsWithSources), [typeof(KeywordSources)])];

	[HarmonyPostfix]
	public static void Postfix(CardModel __instance, KeywordSources sources, ref IReadOnlySet<CardKeyword> __result)
	{
		if (sources.HasFlag(KeywordSources.Global) && !__result.Contains(CardKeyword.Retain) &&
			NagatoCardOrbs.Get(__instance).Any(orb => orb is MountainOrb))
			__result = new HashSet<CardKeyword>(__result) { CardKeyword.Retain };
	}
}
