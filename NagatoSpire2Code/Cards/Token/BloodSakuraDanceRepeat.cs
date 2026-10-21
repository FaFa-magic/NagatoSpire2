using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class BloodSakuraDanceRepeat : NagatoChoiceOption
{
	public CardModel SourceCard { get; set; } = null!;
	public CardPlay SourcePlay { get; set; } = null!;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new HpLossVar(1m)];

	public override async Task OnChosen(PlayerChoiceContext choiceContext)
	{
		if (Owner.PlayerCombatState is not { } state || Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding ||
			SourceCard.HasBeenRemovedFromState || SourceCard.Pile?.IsCombatPile != true)
			return;
		await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue,
			ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, SourcePlay);
		if (Owner.PlayerCombatState == state && !Owner.Creature.IsDead && !CombatManager.Instance.IsOverOrEnding &&
			!SourceCard.HasBeenRemovedFromState && SourceCard.Pile is { IsCombatPile: true, Type: not PileType.Exhaust })
			await CardCmd.AutoPlay(choiceContext, SourceCard, SourcePlay.Target is { IsHittable: true } target ? target : null);
	}

	protected override void OnUpgrade() { }
}
