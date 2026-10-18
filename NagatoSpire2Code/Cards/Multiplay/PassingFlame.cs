using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Multiplay;

public sealed class PassingFlame() : NagatoCardModel(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy), IModelCapabilitySource
{
	public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4m, ValueProp.Move), new IntVar("Orbs", 2)];

	public void BuildDefaultCapabilities(ModelCapabilityList capabilities) =>
		capabilities.Add<CardOrbCapability>().SetOrbs(ModelDb.Orb<FireOrb>().ToMutable());

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(2).FromCard(this, cardPlay)
			.Targeting(cardPlay.Target).WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
		for (int i = 0; i < DynamicVars["Orbs"].IntValue && !CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead; i++)
			await NagatoCardOrbs.Bind(choiceContext, this, ModelDb.Orb<FireOrb>().ToMutable());
	}

	protected override CardLocation GetResultLocationForCardPlay()
	{
		var result = base.GetResultLocationForCardPlay();
		if (CombatState is not { } combat || result.pileType != PileType.Discard)
			return result;
		var teammates = combat.GetTeammatesOf(Owner.Creature)
			.Where(creature => creature.IsAlive && creature.IsPlayer && creature.Player != Owner).ToArray();
		if (teammates.Length == 0 || Owner.RunState.Rng.CombatTargets.NextItem(teammates)?.Player is not { } player)
			return result;
		return result with
		{
			player = player,
			pileType = PileType.Hand,
			position = CardPilePosition.Bottom
		};
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
