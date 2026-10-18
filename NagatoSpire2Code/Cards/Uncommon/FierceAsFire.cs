using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class FierceAsFire() : NagatoCardModel(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(11m, ValueProp.Move), new EnergyVar(1)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Residence, .. ModelDb.Orb<FireOrb>().HoverTips];
	protected override IEnumerable<string> ExtraRunAssetPaths => [.. base.ExtraRunAssetPaths, .. ModelDb.Orb<FireOrb>().AssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState == null)
			return;
		var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).TargetingAllOpponents(CombatState)
			.WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
		int count = attack.Results.SelectMany(results => results).Select(result => result.Receiver)
			.Where(creature => creature.Side == CombatSide.Enemy).Distinct().Count();
		for (int i = 0; i < count && !CombatManager.Instance.IsOverOrEnding && !Owner.Creature.IsDead; i++)
		{
			await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
			await NagatoCardOrbs.Bind(choiceContext, this, ModelDb.Orb<FireOrb>().ToMutable());
		}
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4m);
}
