using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class ReincarnationsEnd() : NagatoCardModel(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new RepeatVar(3)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Residence, .. ModelDb.Orb<WindOrb>().HoverTips];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. ModelDb.Orb<WindOrb>().AssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
				break;
			await NagatoCardOrbs.Bind(choiceContext, this, ModelDb.Orb<WindOrb>().ToMutable());
		}
	}

	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
