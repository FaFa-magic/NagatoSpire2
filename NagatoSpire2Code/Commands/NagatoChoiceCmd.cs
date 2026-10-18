using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Cards;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Commands;

public static class NagatoChoiceCmd
{
	public static async Task Choose(PlayerChoiceContext choiceContext, Player player, IReadOnlyList<CardModel> options)
	{
		if (player.PlayerCombatState is not { } state || player.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;

		bool multiple = player.Creature.GetPower<MyriadRevelationsPower>() != null;
		bool useGrid = multiple || options.Count > 3;
		int maxCount = multiple ? options.Count : 1;
		CardModel[] selected;
		if (useGrid)
			selected = (await CardSelectCmd.FromSimpleGrid(choiceContext, options, player,
				new CardSelectorPrefs(multiple
					? new LocString("powers", "NAGATO_SPIRE2_POWER_MYRIAD_REVELATIONS_POWER.selectionScreenPrompt")
					: new LocString("card_keywords", "NAGATO_SPIRE2_KEYWORD_CHOICE.selectionScreenPrompt"), 1, maxCount)
				{
					RequireManualConfirmation = multiple,
					Cancelable = false
				})).ToArray();
		else
		{
			var option = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, player);
			selected = option == null ? [] : [option];
		}
		if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead || player.PlayerCombatState != state)
			return;

		if (useGrid && (selected.Length < 1 || selected.Length > maxCount || selected.Distinct().Count() != selected.Length))
			throw new InvalidOperationException("A choice must select the permitted number of distinct options.");
		if (selected.Any(card => card is not NagatoChoiceOption || !options.Contains(card)))
			throw new InvalidOperationException("A choice selected a card outside its options.");

		foreach (var choice in options.Where(selected.Contains).Cast<NagatoChoiceOption>())
		{
			if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead || player.PlayerCombatState != state)
				break;

			await Resolve(choiceContext, player, () => choice.OnChosen(choiceContext));
		}
	}

	public static async Task Resolve(PlayerChoiceContext choiceContext, Player player, Func<Task> apply)
	{
		if (player.PlayerCombatState is not { } state || player.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		var listeners = player.Creature.Powers.Where(power => power is INagatoChoiceListener).ToArray();
		await apply();
		foreach (var power in listeners)
		{
			if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead || player.PlayerCombatState != state)
				break;
			if (player.Creature.Powers.Contains(power))
				await ((INagatoChoiceListener)power).AfterNagatoChoice(choiceContext);
		}
	}
}
