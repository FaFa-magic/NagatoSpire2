using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class OtherworldPact() : NagatoCardModel(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Residence, .. ModelDb.Orb<ShadowOrb>().HoverTips];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. ModelDb.Orb<ShadowOrb>().AssetPaths];

	public override Task BeforeCombatStart() => Pile?.IsCombatPile == true
		? NagatoCardOrbs.Bind(new BlockingPlayerChoiceContext(), this, ModelDb.Orb<ShadowOrb>().ToMutable())
		: Task.CompletedTask;

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

	protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
