using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class SalvoFire() : NagatoCardModel(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<SalvoFirePower>(5m)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[NagatoHoverTips.Salvo, HoverTipFactory.FromPower<PlatingPower>()];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) =>
		await PowerCmd.Apply<SalvoFirePower>(choiceContext, Owner.Creature,
			DynamicVars["SalvoFirePower"].BaseValue, Owner.Creature, this);

	protected override void OnUpgrade() => DynamicVars["SalvoFirePower"].UpgradeValueBy(2m);
}
