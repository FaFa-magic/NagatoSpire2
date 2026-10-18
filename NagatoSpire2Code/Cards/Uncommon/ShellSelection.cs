using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Keywords;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class ShellSelection() : NagatoCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		NagatoHoverTips.Loading,
		NagatoHoverTips.Residence,
		HoverTipFactory.FromCard<Shell>(),
		HoverTipFactory.FromOrb<WindOrb>(),
		HoverTipFactory.FromOrb<ForestOrb>(),
		HoverTipFactory.FromOrb<FireOrb>(),
		HoverTipFactory.FromOrb<MountainOrb>()
	];

	protected override IEnumerable<string> ExtraRunAssetPaths =>
	[
		.. base.ExtraRunAssetPaths, .. ModelDb.Card<Shell>().RunAssetPaths, .. NagatoChoiceCmd.AssetPaths,
		.. ModelDb.Orb<WindOrb>().AssetPaths, .. ModelDb.Orb<ForestOrb>().AssetPaths,
		.. ModelDb.Orb<FireOrb>().AssetPaths, .. ModelDb.Orb<MountainOrb>().AssetPaths
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combat || Owner.PlayerCombatState is not { } state ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;
		OrbModel[] orbs = [ModelDb.Orb<WindOrb>(), ModelDb.Orb<ForestOrb>(), ModelDb.Orb<FireOrb>(), ModelDb.Orb<MountainOrb>()];
		var options = orbs.Select(orb =>
		{
			var shell = combat.CreateCard<Shell>(Owner);
			shell.GetOrCreateCapability<CardOrbCapability>().SetOrbs(orb.ToMutable());
			return (CardModel)shell;
		}).ToArray();
		await NagatoChoiceCmd.Choose(choiceContext, Owner, options, async shell =>
		{
			var orb = ModelDb.GetById<OrbModel>(NagatoCardOrbs.Get(shell).Single().Id);
			await CardPileCmd.AddGeneratedCardToCombat(shell, PileType.Play, Owner);
			await NagatoCardSlots.Load(choiceContext, shell);
			if (IsUpgraded && !HasBeenRemovedFromState && Pile?.IsCombatPile == true && Owner.PlayerCombatState == state &&
				!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead)
				await NagatoCardOrbs.Bind(choiceContext, this, orb.ToMutable());
		});
	}

	protected override void OnUpgrade() { }
}
