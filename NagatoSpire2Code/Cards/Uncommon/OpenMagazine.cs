using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class OpenMagazine() : NagatoCardModel(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	public override bool GainsBlock => true;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(15m, ValueProp.Move)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading, NagatoHoverTips.Residence, HoverTipFactory.FromCard<Shell>()];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. ModelDb.Card<Shell>().RunAssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
		if (Owner.PlayerCombatState is not { } state || CombatState is not { } combat ||
			Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		int count = state.Hand.Cards.Sum(card => NagatoCardOrbs.Get(card).Count);
		for (int i = 0; i < count; i++)
		{
			if (Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding || Owner.PlayerCombatState != state)
				break;
			var shell = combat.CreateCard<Shell>(Owner);
			await CardPileCmd.AddGeneratedCardToCombat(shell, PileType.Play, Owner);
			await NagatoCardSlots.Load(choiceContext, shell);
		}
	}

	protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3m);
}
