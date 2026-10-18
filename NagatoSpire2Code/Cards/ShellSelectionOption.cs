using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Cards;

public abstract class ShellSelectionOption : NagatoChoiceOption
{
	public ShellSelection SourceCard { get; set; } = null!;
	protected abstract OrbModel Orb { get; }

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[NagatoHoverTips.Loading, NagatoHoverTips.Residence, HoverTipFactory.FromCard<Shell>(), .. Orb.HoverTips];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. Orb.AssetPaths];

	public override async Task OnChosen(PlayerChoiceContext choiceContext)
	{
		if (Owner.Creature.CombatState is not { } combat || Owner.PlayerCombatState is not { } state ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;
		var shell = combat.CreateCard<Shell>(Owner);
		await NagatoCardOrbs.Bind(choiceContext, shell, Orb.ToMutable());
		await CardPileCmd.AddGeneratedCardToCombat(shell, PileType.Play, Owner);
		await NagatoCardSlots.Load(choiceContext, shell);
		if (SourceCard.IsUpgraded && SourceCard.Owner == Owner && !SourceCard.HasBeenRemovedFromState &&
			SourceCard.Pile?.IsCombatPile == true && Owner.PlayerCombatState == state &&
			!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
			await NagatoCardOrbs.Bind(choiceContext, SourceCard, Orb.ToMutable());
	}

	protected override void OnUpgrade() { }
}
