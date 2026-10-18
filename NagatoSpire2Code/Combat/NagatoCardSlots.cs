using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using NagatoSpire2.NagatoSpire2Code.Capabilities;
using NagatoSpire2.NagatoSpire2Code.Characters;
using NagatoSpire2.NagatoSpire2Code.Nodes.Ui;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public static class NagatoCardSlots
{
	public const int InitialCapacity = 4;
	private static readonly ConditionalWeakTable<PlayerCombatState, Slots> States = new();

	private sealed class Slots(int capacity)
	{
		public int Capacity = capacity;
		public readonly Queue<DrawEntry> Pending = new();
		public readonly Dictionary<CardModel, PlayEntry> Playing = [];
		public CardModel? NextSalvoCard;
		public bool Resolving;
	}

	internal sealed class PlayEntry(CardModel card, PlayerCombatState combat, PlayEntry? previous, bool isSalvo)
	{
		public CardModel Card { get; } = card;
		public PlayerCombatState Combat { get; } = combat;
		public PlayEntry? Previous { get; } = previous;
		public bool IsSalvo { get; } = isSalvo;
		public bool LoadAfterPlay;
	}

	public sealed class DrawEntry(CardModel card)
	{
		public CardModel Card { get; } = card;
		public CardPile? DrawnPile { get; } = card.Pile;
		internal bool Ready;
		internal bool Load;
	}

	private static Slots GetState(Player player) => States.GetValue(player.PlayerCombatState!,
		_ => new Slots(player.Character is NagatoCharacter ? InitialCapacity : 0));

	public static int GetCapacity(Player player)
	{
		if (player.PlayerCombatState is not { } state)
			return 0;
		return player.Character is NagatoCharacter || States.TryGetValue(state, out var slots) && slots.Capacity > 0 || GetCards(player).Count > 0
			? InitialCapacity : 0;
	}

	public static IReadOnlyList<CardModel> GetCards(Player player) =>
		player.PlayerCombatState?.PlayPile.Cards
			.Where(card => !card.HasBeenRemovedFromState && GetSlot(card) > 0).OrderBy(GetSlot).ToArray() ?? [];

	private static int GetSlot(CardModel card) => ModelCapabilities.TryGet(card, out var capabilities)
		? capabilities.Get<LoadedCardCapability>()?.SlotIndex ?? 0 : 0;

	public static bool IsSalvo(CardModel card) => card.Owner?.PlayerCombatState is { } combat &&
		States.TryGetValue(combat, out var slots) && slots.Playing.TryGetValue(card, out var play) && play.IsSalvo;

	internal static PlayEntry? BeginPlay(CardModel card)
	{
		if (card.Owner?.PlayerCombatState is not { } combat)
			return null;
		var slots = GetState(card.Owner);
		bool isSalvo = slots.NextSalvoCard == card;
		if (isSalvo)
			slots.NextSalvoCard = null;
		var entry = new PlayEntry(card, combat, slots.Playing.GetValueOrDefault(card), isSalvo);
		slots.Playing[card] = entry;
		return entry;
	}

	internal static void EndPlay(PlayEntry entry)
	{
		if (!States.TryGetValue(entry.Combat, out var slots) || slots.Playing.GetValueOrDefault(entry.Card) != entry)
			return;
		if (entry.Previous is { } previous)
			slots.Playing[entry.Card] = previous;
		else
			slots.Playing.Remove(entry.Card);
	}

	internal static async Task FinishPlay(PlayerChoiceContext choiceContext, PlayEntry entry)
	{
		EndPlay(entry);
		if (!entry.LoadAfterPlay || entry.Card.Affliction is Sealed { IsAutoplaying: true } ||
			!CanResolve(entry.Card.Owner, entry.Combat))
			return;
		if (entry.Previous is { } previous)
			previous.LoadAfterPlay = true;
		else
			await Load(choiceContext, entry.Card);
	}

	public static DrawEntry? BeginDraw(CardModel card)
	{
		if (card.Owner?.PlayerCombatState == null)
			return null;
		var entry = new DrawEntry(card);
		GetState(card.Owner).Pending.Enqueue(entry);
		return entry;
	}

	public static async Task FinishDraw(PlayerChoiceContext choiceContext, DrawEntry entry)
	{
		var card = entry.Card;
		if (card.Owner.PlayerCombatState is not { } state)
			return;
		entry.Load = card.Affliction is Sealed && !card.HasBeenRemovedFromState &&
			(card.Pile == state.Hand || card.Pile == state.DiscardPile && entry.DrawnPile == state.DiscardPile);
		if (entry.Load)
			entry.Load = await Stage(card);
		entry.Ready = true;
		await Resolve(choiceContext, card.Owner, state);
	}

	public static void CancelDraw(DrawEntry entry)
	{
		entry.Load = false;
		entry.Ready = true;
	}

	public static async Task Load(PlayerChoiceContext choiceContext, CardModel card)
	{
		if (card.Owner.PlayerCombatState is not { } state || !CanResolve(card.Owner, state))
			return;
		var slots = GetState(card.Owner);
		if (slots.Playing.TryGetValue(card, out var play))
		{
			play.LoadAfterPlay = true;
			return;
		}
		if (GetCards(card.Owner).Contains(card) || slots.Pending.Any(entry => entry.Card == card && entry.Load))
			return;
		var entry = new DrawEntry(card);
		slots.Pending.Enqueue(entry);
		entry.Load = await Stage(card);
		entry.Ready = true;
		await Resolve(choiceContext, card.Owner, state);
	}

	private static async Task<bool> Stage(CardModel card)
	{
		var player = card.Owner;
		if (player.PlayerCombatState is not { } state || !CanResolve(player, state) || card.HasBeenRemovedFromState)
			return false;
		var slots = GetState(player);
		if (slots.Capacity == 0)
			slots.Capacity = InitialCapacity;
		NNagatoCardSlots.RemoveHandVisual(card);
		if (card.Pile == state.PlayPile)
			return true;
		return (await CardPileCmd.Add(card, PileType.Play, skipVisuals: true)).success && card.Pile == state.PlayPile;
	}

	private static async Task Resolve(PlayerChoiceContext choiceContext, Player player, PlayerCombatState state)
	{
		var slots = GetState(player);
		if (slots.Resolving)
			return;
		slots.Resolving = true;
		try
		{
			while (CanResolve(player, state) && slots.Pending.TryPeek(out var entry) && entry.Ready)
			{
				slots.Pending.Dequeue();
				var loaded = GetCards(player);
				if (!entry.Load || !IsStored(entry.Card, state) || loaded.Contains(entry.Card))
					continue;
				for (int i = 0; i < loaded.Count; i++)
					loaded[i].Capability<LoadedCardCapability>()!.SetSlot(i + 1);
				entry.Card.GetOrCreateCapability<LoadedCardCapability>().SetSlot(loaded.Count + 1);
				if (loaded.Count + 1 < slots.Capacity)
					continue;
				var batch = GetCards(player);
				foreach (var card in batch)
					card.Capability<LoadedCardCapability>()!.SetSlot(0);
				foreach (var card in batch)
				{
					if (!CanResolve(player, state))
						break;
					if (!IsStored(card, state))
						continue;
					slots.NextSalvoCard = card;
					try
					{
						if (card.Affliction is Sealed seal)
							await seal.Play(choiceContext);
						else
							await CardCmd.AutoPlay(choiceContext, card, null);
					}
					finally
					{
						slots.NextSalvoCard = null;
					}
				}
			}
		}
		finally
		{
			slots.Resolving = false;
		}
	}

	private static bool IsStored(CardModel card, PlayerCombatState state) =>
		!card.HasBeenRemovedFromState && card.Pile == state.PlayPile;

	private static bool CanResolve(Player player, PlayerCombatState state) =>
		player.PlayerCombatState == state && !player.Creature.IsDead && !CombatManager.Instance.IsOverOrEnding;

	public static void Clear(Player player)
	{
		foreach (var card in GetCards(player))
			card.Capability<LoadedCardCapability>()!.SetSlot(0);
		if (player.PlayerCombatState is { } state)
			States.Remove(state);
	}

	public static void AfterCardChangedPiles(CardModel card)
	{
		if (card.Pile?.Type != PileType.Play && ModelCapabilities.TryGet(card, out var capabilities))
			capabilities.Get<LoadedCardCapability>()?.SetSlot(0);
	}
}
