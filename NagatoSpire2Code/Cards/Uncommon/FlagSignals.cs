using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Keywords;
using NagatoSpire2.NagatoSpire2Code.Nodes.Screens;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class FlagSignals() : NagatoCardModel(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice, CardKeyword.Exhaust];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3), new EnergyVar(2)];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<FlagSignalsDraw>(IsUpgraded),
		HoverTipFactory.FromCard<FlagSignalsEnergy>(IsUpgraded)
	];

	protected override IEnumerable<string> ExtraRunAssetPaths =>
		[.. base.ExtraRunAssetPaths, .. NNagatoFleetVoteScreen.AssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combatState || Owner.PlayerCombatState == null ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		NagatoFleetChoiceOption[] options =
		[
			(NagatoFleetChoiceOption)combatState.CreateCard<FlagSignalsDraw>(Owner),
			(NagatoFleetChoiceOption)combatState.CreateCard<FlagSignalsEnergy>(Owner)
		];
		if (IsUpgraded)
			foreach (var option in options)
				CardCmd.Upgrade(option);

		await NagatoFleetVoteCmd.Choose(choiceContext, this, options);
	}

	protected override void OnUpgrade()
	{
		DynamicVars.Cards.UpgradeValueBy(1m);
		DynamicVars.Energy.UpgradeValueBy(1m);
	}
}
