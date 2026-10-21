using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using STS2RitsuLib.Combat.Rewards;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Rewards;

public sealed class CardSealReward(Player player) : ModCustomReward(player)
{
	private static ModRewardDefinition? _definition;

	public static void Register() => _definition ??= ModRewardRegistry.For(MainFile.ModId)
		.RegisterOwned("CardSeal", (_, owner, _) => new CardSealReward(owner));

	public override RewardType ModRewardType => _definition!.RewardType;
	protected override string DescriptionLocKey => "NAGATO_SPIRE2_REWARD_CARD_SEAL";
	protected override string RewardIconPath => "res://NagatoSpire2/images/powers/big/NagatosEdictPower.png";
	protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromAffliction<Sealed>();

	protected override async Task<bool> OnSelect()
	{
		var affliction = ModelDb.Affliction<Sealed>();
		if (!Player.Deck.Cards.Any(affliction.CanAfflict))
			return true;
		var cards = (await CardSelectCmd.FromDeckGeneric(Player,
			new CardSelectorPrefs(new LocString("gameplay_ui", "NAGATO_SPIRE2_REWARD_CARD_SEAL.selectionScreenPrompt"), 1)
			{
				RequireManualConfirmation = true
			}, affliction.CanAfflict)).ToArray();
		foreach (var card in cards)
			if (card.Owner == Player && card.Pile == Player.Deck && affliction.CanAfflict(card))
				card.GetOrCreateCapability<PermanentSealedCapability>();
		return cards.Length > 0;
	}

	public override void MarkContentAsSeen() { }
}
