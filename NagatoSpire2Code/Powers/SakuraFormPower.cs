using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Nodes;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

[RegisterPower]
public sealed class SakuraFormPower : ModPowerTemplate
{
	private const int MaxActivationsPerTurn = 3;

	private sealed class Data
	{
		public int Used;
		public readonly HashSet<CardPlay> Pending = [];
	}

	private SakuraFormBackground? _background;

	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Single;
	public override bool ShouldPlayVfx => false;
	public override PowerAssetProfile AssetProfile => new(
		IconPath: "res://images/powers/echo_form_power.png",
		BigIconPath: "res://images/powers/echo_form_power.png");

	protected override object InitInternalData() => new Data();

	public override Task AfterApplied(Creature? applier, CardModel? cardSource)
	{
		_background = SakuraFormBackground.Create();
		return Task.CompletedTask;
	}

	public override Task AfterRemoved(Creature oldOwner)
	{
		_background?.FadeOut();
		_background = null;
		return Task.CompletedTask;
	}

	public override Task BeforeSideTurnStart(
		PlayerChoiceContext choiceContext,
		CombatSide side,
		IReadOnlyList<Creature> participants,
		ICombatState combatState)
	{
		if (participants.Contains(Owner))
		{
			Data data = GetInternalData<Data>();
			data.Used = 0;
			data.Pending.Clear();
		}
		return Task.CompletedTask;
	}

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		Data data = GetInternalData<Data>();
		if (cardPlay.Player == Owner.Player &&
			!cardPlay.IsAutoPlay &&
			cardPlay.IsFirstInSeries &&
			data.Used < MaxActivationsPerTurn)
		{
			data.Used++;
			data.Pending.Add(cardPlay);
		}
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (!GetInternalData<Data>().Pending.Remove(cardPlay) ||
			Owner.IsDead ||
			!CombatManager.Instance.IsInProgress ||
			Owner.Player is not { } player)
			return;

		Flash();
		int handCount = PileType.Hand.GetPile(player).Cards.Count;
		if (handCount == 0)
			return;

		List<CardModel> selected = (await CardSelectCmd.FromHand(
			choiceContext,
			player,
			new CardSelectorPrefs(SelectionScreenPrompt, 0, handCount),
			null,
			this)).ToList();
		if (selected.Count == 0)
			return;

		IReadOnlyList<CardPileAddResult> results = await CardPileCmd.Add(
			selected,
			PileType.Draw,
			CardPilePosition.Random);
		int moved = results.Count(result => result.success);
		if (moved > 0)
			await CardPileCmd.Draw(choiceContext, moved, player);
	}
}
