using Godot;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace NagatoSpire2.NagatoSpire2Code.Characters;

public sealed class NagatoCardPool : TypeListCardPoolModel, IModColorfulPhilosophersCardPool
{
    public override string Title => NagatoCharacter.CharacterId;
    public override string EnergyColorName => NagatoCharacter.CharacterId;
    
    public override string BigEnergyIconPath => 
        "res://NagatoSpire2/images/packed/sprite_fonts/Nagato_energy_icon_original.png";
    public override string TextEnergyIconPath => 
        "res://NagatoSpire2/images/packed/sprite_fonts/Nagato_energy_icon.png";
    
    public override Color DeckEntryCardColor => new("FFB2FF");
    public override Color EnergyOutlineColor => new("FFB2FF");
    
    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateUnmodulatedHsvShaderMaterial();
    public override Material? PoolFrameMaterial => _poolFrameMaterial;
    
    public override bool IsColorless => false;
}
