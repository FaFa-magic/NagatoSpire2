using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Patching.Models;

namespace NagatoSpire2.NagatoSpire2Code.Patches;

public sealed class NagatoOrbDamagePatch : IPatchMethod
{
	private static readonly AsyncLocal<OrbModel?> Source = new();
	public static OrbModel? CurrentOrb => Source.Value;
	public static string PatchId => "nagato_orb_damage_source";
	public static string Description => "Identify native orb damage without changing its damage properties";
	public static bool IsCritical => true;

	public static ModPatchTarget[] GetTargets() =>
	[
		new(typeof(CreatureCmd), nameof(CreatureCmd.Damage),
			[typeof(PlayerChoiceContext), typeof(IEnumerable<Creature>), typeof(decimal), typeof(ValueProp),
				typeof(Creature), typeof(CardModel), typeof(CardPlay)])
	];

	[HarmonyPrefix]
	public static void Prefix(PlayerChoiceContext choiceContext, Creature? dealer, CardModel? cardSource,
		CardPlay? cardPlay, out OrbModel? __state)
	{
		__state = Source.Value;
		Source.Value = cardSource == null && cardPlay == null &&
			choiceContext.LastInvolvedModel is OrbModel orb && orb.Owner.Creature == dealer ? orb : null;
	}

	[HarmonyFinalizer]
	public static void Finalizer(OrbModel? __state) => Source.Value = __state;
}
