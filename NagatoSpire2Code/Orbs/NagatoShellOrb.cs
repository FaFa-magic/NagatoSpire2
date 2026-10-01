using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Orbs;

public abstract class NagatoShellOrb : ModOrbTemplate
{
	public override decimal PassiveVal => 0m;
	public override ModOrbValueDisplayMode ValueDisplayMode => ModOrbValueDisplayMode.SingleEvoke;
	public override bool AllowInRandomOrbPool => false;
	public override Color DarkenedColor => new("#624B63");
}
