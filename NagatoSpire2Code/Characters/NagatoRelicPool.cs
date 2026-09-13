using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Characters;

public sealed class NagatoRelicPool  : TypeListRelicPoolModel
{
    public override string EnergyColorName => NagatoCharacter.CharacterId;

    public override string BigEnergyIconPath =>
        "res://NagatoSpire2/images/packed/sprite_fonts/Nagato_energy_icon_original.png";
    public override string TextEnergyIconPath =>
        "res://NagatoSpire2/images/packed/sprite_fonts/Nagato_energy_icon.png";
}
