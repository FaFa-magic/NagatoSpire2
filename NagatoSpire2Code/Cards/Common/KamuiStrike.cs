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

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class KamuiStrike() : NagatoCardModel(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];
	protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move), new IntVar("Shells", 2)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading, HoverTipFactory.FromCard<Shell>()];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. ModelDb.Card<Shell>().RunAssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != this || Owner.PlayerCombatState is not { } state || CombatState is not { } combat ||
			Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		for (int i = 0; i < DynamicVars["Shells"].IntValue; i++)
		{
			if (Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding || Owner.PlayerCombatState != state)
				break;
			var shell = combat.CreateCard<Shell>(Owner);
			await CardPileCmd.AddGeneratedCardToCombat(shell, PileType.Play, Owner);
			await NagatoCardSlots.Load(choiceContext, shell);
		}
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
