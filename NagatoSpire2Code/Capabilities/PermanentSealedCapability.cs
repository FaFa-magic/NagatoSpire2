using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Capabilities;

[RegisterModelCapability]
public sealed class PermanentSealedCapability : CardCapability, ICardDescriptionContributor, ICardHoverTipContributor,
	ICardOverlayAssetPathContributor, ICardOverlayContributor
{
	public override void Attach(AbstractModel owner, bool isInternal = false)
	{
		base.Attach(owner, isInternal);
		if (owner is not CardModel { IsMutable: true } card || card.Affliction is Sealed)
			return;
		card.ClearAfflictionInternal();
		var affliction = ModelDb.Affliction<Sealed>().ToMutable();
		affliction.Amount = 1;
		card.AfflictInternal(affliction, 1m);
		affliction.AfterApplied();
	}

	public IEnumerable<CardDescriptionFragment> GetDescriptionFragments(CardDescriptionContext context) =>
		context.Card.Affliction is Sealed ? [] :
		[new CardDescriptionFragment(new LocString("afflictions", $"{ModelDb.Affliction<Sealed>().Id.Entry}.cardText"))];

	public IEnumerable<IHoverTip> GetHoverTips(CardModel card) =>
		card.Affliction is Sealed ? [] : HoverTipFactory.FromAffliction<Sealed>();

	public IEnumerable<CardOverlayContribution> GetCardOverlays(CardOverlayContext context) =>
		context.Card.Affliction is Sealed ? [] :
		[CardOverlayContribution.FromScenePath("permanent_sealed", NSealedCardVfx.ScenePath)];

	public IEnumerable<string> GetCardOverlayAssetPaths(CardModel card) => [NSealedCardVfx.ScenePath];
}
