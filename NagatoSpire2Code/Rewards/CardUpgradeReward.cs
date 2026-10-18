using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using STS2RitsuLib.Combat.Rewards;

namespace NagatoSpire2.NagatoSpire2Code.Rewards;

public sealed class CardUpgradeReward(Player player) : ModCustomReward(player)
{
	private static ModRewardDefinition? _definition;

	public static void Register() => _definition ??= ModRewardRegistry.For(MainFile.ModId)
		.RegisterOwned("CardUpgrade", (_, owner, _) => new CardUpgradeReward(owner));

	public override RewardType ModRewardType => _definition!.RewardType;
	protected override string DescriptionLocKey => "NAGATO_SPIRE2_REWARD_CARD_UPGRADE";
	protected override string RewardIconPath => "res://images/ui/cards/upgrade_preview/upgrade_arrow.png";

	protected override async Task<bool> OnSelect()
	{
		if (!Player.Deck.Cards.Any(card => card.IsUpgradable))
			return true;
		var cards = (await CardSelectCmd.FromDeckForUpgrade(Player,
			new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1)
			{
				RequireManualConfirmation = true
			})).ToArray();
		foreach (var card in cards)
			CardCmd.Upgrade(card);
		return cards.Length > 0;
	}

	public override void MarkContentAsSeen() { }
}
