using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Characters;
using NagatoSpire2.NagatoSpire2Code.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Basic;

[RegisterCharacterStarterCard(typeof(NagatoCharacter), 1, Order = 3)]
public sealed class TargetedReload() : NagatoOrbSlotCardModel(1, CardType.Skill, CardRarity.Basic)
{
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromPower<PlatingPower>()];

	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<TargetedReloadPlatingPower>(6m)];

	protected override async Task OnPlayWithOrbSlot(PlayerChoiceContext choiceContext, CardPlay cardPlay, int? slotIndex)
	{
		await PowerCmd.Apply<TargetedReloadPlatingPower>(choiceContext, Owner.Creature,
			DynamicVars["TargetedReloadPlatingPower"].BaseValue, Owner.Creature, this);
	}

	protected override void OnUpgrade() => DynamicVars["TargetedReloadPlatingPower"].UpgradeValueBy(3m);
}
