using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Keywords;
using NagatoSpire2.NagatoSpire2Code.Nodes.Screens;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class CombinedFleet() : NagatoCardModel(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(1)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromCard<CombinedFleetStrength>(),
		HoverTipFactory.FromCard<CombinedFleetDexterity>(),
		HoverTipFactory.FromCard<CombinedFleetDraw>(),
		HoverTipFactory.FromCard<CombinedFleetEnergy>()
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
			(NagatoFleetChoiceOption)combatState.CreateCard<CombinedFleetStrength>(Owner),
			(NagatoFleetChoiceOption)combatState.CreateCard<CombinedFleetDexterity>(Owner),
			(NagatoFleetChoiceOption)combatState.CreateCard<CombinedFleetDraw>(Owner),
			(NagatoFleetChoiceOption)combatState.CreateCard<CombinedFleetEnergy>(Owner)
		];
		await NagatoFleetVoteCmd.Choose(choiceContext, this, options);
	}

	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
