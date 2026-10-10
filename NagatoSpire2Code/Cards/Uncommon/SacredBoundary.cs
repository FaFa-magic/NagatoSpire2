using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class SacredBoundary() : NagatoCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => HoverTipFactory.FromAffliction<Sealed>();

	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, NSealedCardVfx.ScenePath];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (Owner.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		var affliction = ModelDb.Affliction<Sealed>();
		var selected = (await CardSelectCmd.FromCombatPile(choiceContext, state.DrawPile, Owner,
			new CardSelectorPrefs(SelectionScreenPrompt, 0, DynamicVars.Cards.IntValue)
			{
				RequireManualConfirmation = true,
				Cancelable = false
			}, affliction.CanAfflict)).ToArray();
		foreach (var card in selected)
		{
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState != state)
				break;
			if (card.Pile == state.DrawPile && affliction.CanAfflict(card))
				await CardCmd.Afflict<Sealed>(card, 1m);
		}
	}

	protected override void OnUpgrade() => DynamicVars.Cards.UpgradeValueBy(1m);
}
