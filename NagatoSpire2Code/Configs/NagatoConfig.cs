using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils;
using STS2RitsuLib.Utils.Persistence;

namespace NagatoSpire2.NagatoSpire2Code.Configs;

public enum NagatoModelMode
{
    手部模型,
    腿部模型
}

public sealed class NagatoConfig
{
    public NagatoModelMode 多人模式使用哪种模型 { get; set; } = NagatoModelMode.手部模型;
    public bool 进入第四幕 { get; set; } = true;
}

public static class NagatoConfigPage
{
    private const string DataKey = "NagatoConfig";
    private static readonly I18N Localization = RitsuLibFramework.CreateModLocalization(
        MainFile.ModId,
        "NagatoConfig",
        resourceFolders: ["NagatoSpire2.Settings"]);

    public static readonly ModSettingsValueBinding<NagatoConfig, NagatoModelMode> ModelModeBinding = new(
        MainFile.ModId, DataKey, SaveScope.Profile,
        static s => s.多人模式使用哪种模型,
        static (s, v) => s.多人模式使用哪种模型 = v);

    public static readonly ModSettingsValueBinding<NagatoConfig, bool> FourthActBinding = new(
        MainFile.ModId, DataKey, SaveScope.Profile,
        static s => s.进入第四幕,
        static (s, v) => s.进入第四幕 = v);

    public static void Register()
    {
        ModDataStore.For(MainFile.ModId).Register<NagatoConfig>(
            key: DataKey,
            fileName: "settings.json",
            scope: SaveScope.Profile,
            defaultFactory: () => new NagatoConfig(),
            autoCreateIfMissing: true);

        RitsuLibFramework.RegisterModSettings(MainFile.ModId, page => page
            .WithTitle(Text("config.page.title", "Nagato Settings"))
            .WithModDisplayName(Text("config.modDisplayName", "Nagato"))
            .WithVisibleOnHostSurfaces(
                ModSettingsHostSurface.MainMenu | ModSettingsHostSurface.RunPause)
            .AddSection("general", section => section
                .WithTitle(Text("config.section.general", "General"))
                .AddChoice("mosaic_mode", Text("config.modelMode.label", "Multiplayer model"),
                    ModelModeBinding,
                    [
                        new(NagatoModelMode.手部模型, Text("config.modelMode.hand", "Hand model")),
                        new(NagatoModelMode.腿部模型, Text("config.modelMode.legs", "Leg model"))
                    ],
                    presentation: ModSettingsChoicePresentation.Dropdown)
                .AddToggle("fourth_act", Text("config.fourthAct.label", "Enter the fourth act"),
                    FourthActBinding,
                    Text("config.fourthAct.description", "Applies to new runs containing Nagato. Multiplayer uses the host's setting. Saved runs retain their original route."))));
    }

    private static ModSettingsText Text(string key, string fallback) =>
        ModSettingsText.I18N(Localization, key, fallback);
}
