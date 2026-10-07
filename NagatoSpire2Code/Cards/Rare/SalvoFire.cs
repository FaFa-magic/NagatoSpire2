using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Rare;

public sealed class SalvoFire() : NagatoCardModel(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		HoverTipFactory.Static(StaticHoverTip.Evoke),
		HoverTipFactory.FromOrb<HighExplosiveShellOrb>()
	];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		await PowerCmd.Apply<SalvoFirePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
	}

	protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}
