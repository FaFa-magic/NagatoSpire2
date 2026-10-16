using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Common;

public sealed class AntiAircraftBattery() : NagatoCardModel(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy), IModelCapabilitySource
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Move), new IntVar("Hits", 3)];

	public void BuildDefaultCapabilities(ModelCapabilityList capabilities) =>
		capabilities.Add<CardOrbCapability>().SetOrbs(ModelDb.Orb<FireOrb>().ToMutable());

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
		await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(DynamicVars["Hits"].IntValue)
			.FromCard(this, cardPlay).Targeting(cardPlay.Target)
			.WithHitFx("vfx/vfx_attack_slash").Execute(choiceContext);
	}

	protected override void OnUpgrade() => DynamicVars["Hits"].UpgradeValueBy(1m);
}
