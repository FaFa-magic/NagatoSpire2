using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards.Ancient;
using NagatoSpire2.NagatoSpire2Code.Characters;
using NagatoSpire2.NagatoSpire2Code.Commands;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Basic;

[RegisterCharacterStarterCard(typeof(NagatoCharacter), 1, Order = 2)]
[RegisterArchaicToothTranscendence(typeof(SakuraKagura))]
public sealed class SakuraDance() : NagatoCardModel(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Ethereal];

	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.Static(StaticHoverTip.Evoke)];

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != this || Owner.PlayerCombatState is not { } state || state.OrbQueue.Orbs.Count == 0)
			return;

		await NagatoOrbCmd.Evoke(choiceContext, Owner, [state.OrbQueue.Orbs[^1], state.OrbQueue.Orbs[0]]);
	}

	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay) => Task.CompletedTask;

	protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
