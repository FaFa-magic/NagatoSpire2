using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Keywords;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class ShellSelection() : NagatoCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		NagatoHoverTips.Loading,
		HoverTipFactory.FromCard<ShellSelectionWind>(IsUpgraded),
		HoverTipFactory.FromCard<ShellSelectionForest>(IsUpgraded),
		HoverTipFactory.FromCard<ShellSelectionFire>(IsUpgraded),
		HoverTipFactory.FromCard<ShellSelectionMountain>(IsUpgraded)
	];

	protected override IEnumerable<string> ExtraRunAssetPaths =>
	[
		.. base.ExtraRunAssetPaths, .. ModelDb.Card<Shell>().RunAssetPaths,
		.. ModelDb.Card<ShellSelectionWind>().RunAssetPaths, .. ModelDb.Card<ShellSelectionForest>().RunAssetPaths,
		.. ModelDb.Card<ShellSelectionFire>().RunAssetPaths, .. ModelDb.Card<ShellSelectionMountain>().RunAssetPaths
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combat || Owner.PlayerCombatState == null ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;
		ShellSelectionOption[] options =
		[
			(ShellSelectionOption)combat.CreateCard<ShellSelectionWind>(Owner),
			(ShellSelectionOption)combat.CreateCard<ShellSelectionForest>(Owner),
			(ShellSelectionOption)combat.CreateCard<ShellSelectionFire>(Owner),
			(ShellSelectionOption)combat.CreateCard<ShellSelectionMountain>(Owner)
		];
		foreach (var option in options)
		{
			option.SourceCard = this;
			if (IsUpgraded)
				CardCmd.Upgrade(option);
		}
		await NagatoChoiceCmd.Choose(choiceContext, Owner, options);
	}

	protected override void OnUpgrade() { }
}
