using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class EvergreenWard() : NagatoCardModel(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
	public override bool GainsBlock => true;
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(6m, ValueProp.Move)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Residence, .. ModelDb.Orb<ForestOrb>().HoverTips];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. ModelDb.Orb<ForestOrb>().AssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		if (Owner.PlayerCombatState is not { } state || Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		var selected = (await CardSelectCmd.FromHand(choiceContext, Owner,
			new CardSelectorPrefs(SelectionScreenPrompt, 1), filter: card => NagatoCardOrbs.Get(card).Count == 0, source: this)).FirstOrDefault();
		if (!Owner.Creature.IsDead && !CombatManager.Instance.IsOverOrEnding && Owner.PlayerCombatState == state &&
			selected?.Owner == Owner && selected.Pile == state.Hand && NagatoCardOrbs.Get(selected).Count == 0)
			await NagatoCardOrbs.Bind(choiceContext, selected, ModelDb.Orb<ForestOrb>().ToMutable());
	}

	protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
