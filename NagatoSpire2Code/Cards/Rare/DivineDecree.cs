using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Keywords;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class DivineDecree() : NagatoCardModel(0, CardType.Skill, CardRarity.Rare, TargetType.Self), IModelCapabilitySource
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice, CardKeyword.Exhaust];

	public void BuildDefaultCapabilities(ModelCapabilityList capabilities) =>
		capabilities.Add<CardOrbCapability>().SetOrbs(ModelDb.Orb<ShadowOrb>().ToMutable());

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.FromPower<DivineDecreeGuardPower>(),
		HoverTipFactory.FromPower<DivineDecreeWrathPower>()
	];

	protected override IEnumerable<string> ExtraRunAssetPaths =>
	[
		.. base.ExtraRunAssetPaths, .. NagatoChoiceCmd.AssetPaths, .. ModelDb.Orb<WindOrb>().AssetPaths,
		.. ModelDb.Card<DivineDecreeGuard>().RunAssetPaths, .. ModelDb.Card<DivineDecreeWrath>().RunAssetPaths
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combatState || Owner.PlayerCombatState == null ||
			CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead)
			return;

		CardModel[] options =
		[
			combatState.CreateCard<DivineDecreeGuard>(Owner),
			combatState.CreateCard<DivineDecreeWrath>(Owner)
		];

		await NagatoChoiceCmd.Choose(choiceContext, Owner, options);
	}

	protected override void OnUpgrade() => NagatoCardOrbs.BindOnUpgrade(this, ModelDb.Orb<WindOrb>().ToMutable());
}
