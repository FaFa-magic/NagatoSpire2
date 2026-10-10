using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class PiercingOnslaught() : NagatoCardModel(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move), new IntVar("Shells", 3)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[HoverTipFactory.Static(StaticHoverTip.Evoke), HoverTipFactory.FromOrb<ArmorPiercingShellOrb>()];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		if (Owner.PlayerCombatState is not { } state)
			return;
		int before = NagatoOrbHistory.GetEvokeCount<ArmorPiercingShellOrb>(Owner);
		do
		{
			await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
				.WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
			if (CombatManager.Instance.IsOverOrEnding || Owner.Creature.IsDead || Owner.PlayerCombatState != state ||
				state.OrbQueue.Orbs.LastOrDefault() is not { } orb)
				break;
			await NagatoOrbCmd.Evoke(choiceContext, Owner, [orb]);
		}
		while (!CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead && Owner.PlayerCombatState == state &&
			state.OrbQueue.Orbs.Count > 0 &&
			NagatoOrbHistory.GetEvokeCount<ArmorPiercingShellOrb>(Owner) - before < DynamicVars["Shells"].IntValue);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
