using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class RecklessSalvo() : NagatoCardModel(2, CardType.Attack, CardRarity.Uncommon, TargetType.Self), IModelCapabilitySource
{
	public override bool HasTurnEndInHandEffect => true;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20m, ValueProp.Move), new IntVar("BlockLoss", 15)];

	public void BuildDefaultCapabilities(ModelCapabilityList capabilities) =>
		capabilities.Add<CardOrbCapability>().SetOrbs(ModelDb.Orb<ForestOrb>().ToMutable());

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != this || Owner.PlayerCombatState == null || CombatState is not { } combat ||
			Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding || combat.HittableEnemies.Count == 0)
			return;
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, null).TargetingRandomOpponents(combat)
			.WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
	}

	protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext) =>
		await CreatureCmd.LoseBlock(choiceContext, Owner.Creature, DynamicVars["BlockLoss"].BaseValue, Owner.Creature);

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(6m);
}
