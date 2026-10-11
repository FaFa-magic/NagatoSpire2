using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;
using NagatoSpire2.NagatoSpire2Code.Encounters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Acts;

[RegisterAct]
public sealed class InkStainedSteelSakura : ModActTemplate
{
    public const string MapAssetDirectory = "res://NagatoSpire2/images/map/ink_stained_steel_sakura/";
    public const string BossTitleKey = "NAGATO_SPIRE2_ACT_INK_STAINED_STEEL_SAKURA.boss.title";
    public const string BossHudIconPath = MapAssetDirectory + "nagato_miko_boss.png";
    public const string RestSiteScenePath = "res://NagatoSpire2/scenes/rest_site/ink_stained_steel_sakura_rest_site.tscn";
    private static Glory VanillaAct => ModelDb.Act<Glory>();

    public static EncounterModel Boss => ModelDb.Encounter<NagatoBossEncounter>();
    public static IEnumerable<EncounterModel> InitialEliteEncounters =>
    [
        ModelDb.Encounter<KnightsElite>(),
        ModelDb.Encounter<MechaKnightElite>()
    ];

    public override int Index => 3;
    public override bool IsDefault => false;
    protected override int BaseNumberOfRooms => InkStainedSteelSakuraMap.RoomCount - 1;
    protected override int NumberOfWeakEncounters => 0;
    public override Color MapTraveledColor => new("28231D");
    public override Color MapUntraveledColor => new("877256");
    public override Color MapBgColor => new("A78A67");
    public override string[] BgMusicOptions => VanillaAct.BgMusicOptions;
    public override string[] MusicBankPaths => VanillaAct.MusicBankPaths;
    public override string AmbientSfx => VanillaAct.AmbientSfx;
    public override string ChestSpineSkinNameNormal => VanillaAct.ChestSpineSkinNameNormal;
    public override string ChestSpineSkinNameStroke => VanillaAct.ChestSpineSkinNameStroke;
    public override string ChestOpenSfx => VanillaAct.ChestOpenSfx;
    public override ActAssetProfile AssetProfile => ContentAssetProfiles.FromVanillaActId("glory") with
    {
        ChestSpineResourcePath = VanillaAct.ChestSpineResourcePath,
        RestSiteBackgroundPath = RestSiteScenePath,
        MapTopBgPath = MapAssetDirectory + "map_scroll.png",
        MapMidBgPath = MapAssetDirectory + "map_scroll.png",
        MapBotBgPath = MapAssetDirectory + "map_scroll.png"
    };
    public override IEnumerable<EncounterModel> BossDiscoveryOrder => [Boss];
    public override IEnumerable<AncientEventModel> AllAncients => [];
    public override IEnumerable<EventModel> AllEvents => [];

    public override IEnumerable<EncounterModel> GenerateAllEncounters() => [.. InitialEliteEncounters, Boss];
    public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState unlockState) => [];
    public override bool IsUnlocked(UnlockState unlockState) => true;
    public override MapPointTypeCounts GetMapPointTypes(Rng mapRng) => new(0, 2) { NumOfElites = 2 };

    protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
    {
    }

    public static ActModel CreateForRun()
    {
        InkStainedSteelSakura act = (InkStainedSteelSakura)ModelDb.Act<InkStainedSteelSakura>().ToMutable();
        act._rooms.eliteEncounters.AddRange(InitialEliteEncounters);
        act.SetBossEncounter(Boss);
        return act;
    }
}
