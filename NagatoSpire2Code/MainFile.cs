using System.Reflection;
using MegaCrit.Sts2.Core.Modding;
using NagatoSpire2.NagatoSpire2Code.Patches;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace NagatoSpire2.NagatoSpire2Code;

[ModInitializer(nameof(Initialize))]
public static class MainFile
{
	public const string ModId = "NagatoSpire2";

	public static Logger Logger { get; private set; } = null!;

	public static void Initialize()
	{
		var assembly = Assembly.GetExecutingAssembly();

		Logger = RitsuLibFramework.CreateLogger(ModId);
		ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
		RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);

		var cardVisualPatcher = RitsuLibFramework.CreatePatcher(ModId, "card-visuals");
		cardVisualPatcher.RegisterPatch<NagatoCardChromePatch>();
		if (!cardVisualPatcher.PatchAll())
			Logger.ErrorNoTrace("Nagato card visual patches failed to apply.");
	}
}
