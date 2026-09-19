using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Saves;
using NagatoSpire2.NagatoSpire2Code.Characters;
using STS2RitsuLib;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

/// <summary>
/// Skin variants are persistence identities, not additional gameplay characters.
/// </summary>
public sealed class NagatoSkinEnumerationPatch : IPatchMethod
{
	public static string PatchId => "nagato_skin_character_enumeration";
	public static string Description => "Exclude auxiliary Nagato skin models from global character lists";
	public static bool IsCritical => true;
	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(ModelDb), nameof(ModelDb.AllCharacters), null, MethodType.Getter)
	];

	[HarmonyAfter(Const.FrameworkContentRegistryHarmonyId)]
	[HarmonyPriority(Priority.Last)]
	[HarmonyPostfix]
	public static void Postfix(ref IEnumerable<CharacterModel> __result)
	{
		__result = __result.Where(character =>
			character is not NagatoCharacter nagato || nagato.CurrentSkin == NagatoSkin.Default);
	}
}

public static class NagatoSharedProgression
{
	public static ModelId GetProgressionId(ModelId characterId)
	{
		return IsSkinVariantId(characterId) ? ModelDb.GetId<NagatoCharacter>() : characterId;
	}

	private static bool IsSkinVariantId(ModelId characterId)
	{
		return characterId == ModelDb.GetId<NagatoVariantTwo>()
		       || characterId == ModelDb.GetId<NagatoVariantThree>()
		       || characterId == ModelDb.GetId<NagatoVariantFour>()
		       || characterId == ModelDb.GetId<NagatoVariantFive>()
		       || characterId == ModelDb.GetId<NagatoVariantSix>()
		       || characterId == ModelDb.GetId<NagatoVariantH>();
	}
}

public sealed class NagatoSharedProgressionLookupPatch : IPatchMethod
{
	public static string PatchId => "nagato_skin_shared_progression";
	public static string Description => "Share Nagato progression across Spine skins";
	public static bool IsCritical => true;
	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(ProgressState), nameof(ProgressState.GetOrCreateCharacterStats), [typeof(ModelId)]),
		new(typeof(ProgressState), nameof(ProgressState.GetStatsForCharacter), [typeof(ModelId)])
	];

	[HarmonyPrefix]
	public static void Prefix(ref ModelId characterId)
	{
		characterId = NagatoSharedProgression.GetProgressionId(characterId);
	}
}

public sealed class NagatoSkinAncientDialogueLookupPatch : IPatchMethod
{
	public static string PatchId => "nagato_skin_ancient_dialogue_lookup";
	public static string Description => "Use base Nagato dialogue for every Nagato skin variant";
	public static bool IsCritical => true;
	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(AncientDialogueSet), nameof(AncientDialogueSet.GetValidDialogues),
			[typeof(ModelId), typeof(int), typeof(int), typeof(bool)])
	];

	[HarmonyPrefix]
	public static void Prefix(ref ModelId characterId)
	{
		characterId = NagatoSharedProgression.GetProgressionId(characterId);
	}
}

public sealed class NagatoSharedGameOverProgressionPatch : IPatchMethod
{
	public static string PatchId => "nagato_skin_shared_game_over_progression";
	public static string Description => "Store Nagato skin badges in shared character progression";
	public static bool IsCritical => true;
	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(NGameOverScreen), "SaveBadgesToProgress", null)
	];

	[HarmonyPrefix]
	[HarmonyPriority(Priority.First)]
	public static void Prefix(Player ____localPlayer, out IDisposable? __state)
	{
		__state = null;
		ModelId skinId = ____localPlayer.Character.Id;
		ModelId progressionId = NagatoSharedProgression.GetProgressionId(skinId);
		if (skinId == progressionId)
		{
			return;
		}

		ProgressState progress = SaveManager.Instance.Progress;
		if (progress.CharacterStats is not IDictionary<ModelId, CharacterStats> characterStats)
		{
			throw new InvalidOperationException(
				"The game-over progression dictionary is not mutable; Nagato cannot install its scoped skin alias.");
		}

		CharacterStats sharedStats = progress.GetOrCreateCharacterStats(progressionId);
		bool hadPrevious = characterStats.TryGetValue(skinId, out CharacterStats? previous);
		characterStats[skinId] = sharedStats;
		__state = new CharacterStatsAliasLease(characterStats, skinId, hadPrevious, previous);
	}

	public static void Finalizer(IDisposable? __state)
	{
		__state?.Dispose();
	}

	private sealed class CharacterStatsAliasLease(
		IDictionary<ModelId, CharacterStats> characterStats,
		ModelId skinId,
		bool hadPrevious,
		CharacterStats? previous) : IDisposable
	{
		public void Dispose()
		{
			if (hadPrevious)
			{
				characterStats[skinId] = previous!;
			}
			else
			{
				characterStats.Remove(skinId);
			}
		}
	}
}
