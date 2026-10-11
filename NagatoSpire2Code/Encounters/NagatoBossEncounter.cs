using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using NagatoSpire2.NagatoSpire2Code.Acts;
using NagatoSpire2.NagatoSpire2Code.Monsters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Encounters;

[RegisterActEncounter(typeof(InkStainedSteelSakura))]
public sealed class NagatoBossEncounter : ModEncounterTemplate
{
    public override RoomType RoomType => RoomType.Boss;
    public override bool IsValidForAct(ActModel act) => act is InkStainedSteelSakura;
    public override string BossNodePath => InkStainedSteelSakura.MapAssetDirectory + "nagato_boss_icon";
    public override MegaSkeletonDataResource? BossNodeSpineResource => null;
    public override EncounterAssetProfile AssetProfile => new(
        MapNodeAssetPaths: [BossNodePath + ".png", BossNodePath + "_outline.png"],
        RunHistoryIconPath: InkStainedSteelSakura.BossHudIconPath,
        RunHistoryIconOutlinePath: InkStainedSteelSakura.BossHudIconPath);
    public override IEnumerable<MonsterModel> AllPossibleMonsters => [ModelDb.Monster<NagatoBoss>()];

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters() => [(ModelDb.Monster<NagatoBoss>().ToMutable(), null)];
}
