using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace NagatoSpire2.NagatoSpire2Code.Keywords;

[RegisterOwnedCardKeyword(nameof(Choice), CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
public sealed class NagatoKeywords
{
	public static readonly CardKeyword Choice = ModContentRegistry.GetQualifiedKeywordId(MainFile.ModId, nameof(Choice)).GetModCardKeyword();
}
