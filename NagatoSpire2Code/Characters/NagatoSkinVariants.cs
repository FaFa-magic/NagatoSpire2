using STS2RitsuLib.Interop.AutoRegistration;

namespace NagatoSpire2.NagatoSpire2Code.Characters;

public abstract class NagatoSkinVariant : NagatoCharacter
{
	public sealed override bool HideFromVanillaCharacterSelect => true;
	public sealed override bool AllowInVanillaRandomCharacterSelect => false;
	public sealed override bool HideInCardLibraryCompendium => true;
}

[RegisterCharacter]
public sealed class NagatoVariantTwo : NagatoSkinVariant
{
	public override NagatoSkin CurrentSkin => NagatoSkin.VariantTwo;
}

[RegisterCharacter]
public sealed class NagatoVariantThree : NagatoSkinVariant
{
	public override NagatoSkin CurrentSkin => NagatoSkin.VariantThree;
}

[RegisterCharacter]
public sealed class NagatoVariantFour : NagatoSkinVariant
{
	public override NagatoSkin CurrentSkin => NagatoSkin.VariantFour;
}

[RegisterCharacter]
public sealed class NagatoVariantFive : NagatoSkinVariant
{
	public override NagatoSkin CurrentSkin => NagatoSkin.VariantFive;
}

[RegisterCharacter]
public sealed class NagatoVariantSix : NagatoSkinVariant
{
	public override NagatoSkin CurrentSkin => NagatoSkin.VariantSix;
}

[RegisterCharacter]
public sealed class NagatoVariantH : NagatoSkinVariant
{
	public override NagatoSkin CurrentSkin => NagatoSkin.VariantH;
}
