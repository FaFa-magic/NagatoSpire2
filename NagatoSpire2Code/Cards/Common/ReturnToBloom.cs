using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class ReturnToBloom() : NagatoCardModel(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Residence];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. NagatoOrbPool.AssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		var orbs = NagatoOrbHistory.GetRecentOrbs(Owner);
		foreach (var orb in orbs)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
				break;
			await NagatoCardOrbs.Bind(choiceContext, this, orb);
		}
	}

	protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
