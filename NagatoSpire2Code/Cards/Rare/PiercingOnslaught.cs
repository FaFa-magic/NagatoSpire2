using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class PiercingOnslaught() : NagatoCardModel(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Move), new IntVar("Hits", 8)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading];

	public override Task BeforeCombatStart() => Pile?.IsCombatPile == true
		? NagatoCardSlots.Load(new BlockingPlayerChoiceContext(), this) : Task.CompletedTask;

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars["Hits"].IntValue)
			.FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_blunt").Execute(choiceContext);
	}

	protected override void OnUpgrade() => DynamicVars["Hits"].UpgradeValueBy(2m);
}
