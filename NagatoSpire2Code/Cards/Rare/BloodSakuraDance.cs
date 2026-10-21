using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.Keywords;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class BloodSakuraDance() : NagatoCardModel(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy), IModelCapabilitySource
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];
	protected override IEnumerable<string> ExtraRunAssetPaths =>
	[
		.. base.ExtraRunAssetPaths, .. NagatoChoiceCmd.AssetPaths,
		.. ModelDb.Card<BloodSakuraDanceRepeat>().RunAssetPaths, .. ModelDb.Card<BloodSakuraDanceExhaust>().RunAssetPaths
	];

	public void BuildDefaultCapabilities(ModelCapabilityList capabilities) =>
		capabilities.Add<CardOrbCapability>().SetOrbs(ModelDb.Orb<FireOrb>().ToMutable());

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		if (CombatState is not { } combat || Owner.PlayerCombatState is not { } state ||
			Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
		if (Owner.PlayerCombatState != state || Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		var repeat = combat.CreateCard<BloodSakuraDanceRepeat>(Owner);
		repeat.SourceCard = this;
		repeat.SourcePlay = cardPlay;
		var exhaust = combat.CreateCard<BloodSakuraDanceExhaust>(Owner);
		exhaust.SourceCard = this;
		await NagatoChoiceCmd.Choose(choiceContext, Owner, [repeat, exhaust]);
	}

	protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2m);
}
