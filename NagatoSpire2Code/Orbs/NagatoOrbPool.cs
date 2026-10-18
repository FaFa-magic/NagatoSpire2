using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

public static class NagatoOrbPool
{
	private static readonly ModelId[] OrbIds =
	[
		ModelDb.GetId<WindOrb>(), ModelDb.GetId<ForestOrb>(), ModelDb.GetId<FireOrb>(),
		ModelDb.GetId<MountainOrb>(), ModelDb.GetId<ShadowOrb>(), ModelDb.GetId<ThunderOrb>()
	];

	public static OrbModel CreateRandom(Rng rng) => ModelDb.GetById<OrbModel>(OrbIds[rng.NextInt(OrbIds.Length)]).ToMutable();

	public static IEnumerable<string> AssetPaths => OrbIds.SelectMany(id => ModelDb.GetById<OrbModel>(id).AssetPaths)
		.Concat([NSealedCardVfx.ScenePath]).Distinct();
}
