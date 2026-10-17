using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Afflictions;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Nodes.Vfx;
using NagatoSpire2.NagatoSpire2Code.Orbs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Capabilities;

[RegisterModelCapability]
public sealed class CardOrbCapability : CardCapability, ICardDescriptionContributor, ICardHoverTipContributor,
	ICardOverlayAssetPathContributor, ICardOverlayContributor
{
	public const int MaxOrbs = 3;
	private IReadOnlyList<OrbModel> _orbs = [];
	private int _handFocus;
	private Player? _focusOwner;
	private List<int> _shadowTurns = [];

	public IReadOnlyList<OrbModel> Orbs
	{
		get
		{
			foreach (var orb in _orbs.OfType<NagatoOrbModel>())
				orb.HostCard = Owner;
			if (Owner is { IsMutable: true, Owner: { } player })
			{
				foreach (var orb in _orbs)
					if (orb.Owner != player)
					{
						orb.Owner = null!;
						orb.Owner = player;
					}
			}
			return _orbs;
		}
	}

	public void SetOrbs(params OrbModel[] orbs)
	{
		AssertMutable();
		if (orbs.Length > MaxOrbs || orbs.Distinct().Count() != orbs.Length)
			throw new InvalidOperationException($"A card can host at most {MaxOrbs} distinct orb instances.");
		foreach (var orb in orbs)
			orb.AssertMutable();
		_orbs = Array.AsReadOnly(orbs.ToArray());
		ApplyInnateAffliction();
		if (Owner != null)
			MarkDirty();
	}

	public override IModelCapability CloneFor(AbstractModel clonedOwner)
	{
		var clone = (CardOrbCapability)base.CloneFor(clonedOwner);
		clone._orbs = Array.AsReadOnly(_orbs.Select(orb =>
		{
			var copy = (OrbModel)orb.MutableClone();
			copy.Owner = null!;
			return copy;
		}).ToArray());
		clone._handFocus = 0;
		clone._focusOwner = null;
		clone._shadowTurns = [];
		return clone;
	}

	protected override JsonNode? SaveAdditionalState() => _orbs.Count == 0 && _shadowTurns.Count == 0 && _handFocus == 0 ? null : new JsonObject
	{
		["orbs"] = new JsonArray(_orbs.Select(orb => (JsonNode?)JsonValue.Create(orb.Id.ToString())).ToArray()),
		["handFocus"] = _handFocus,
		["shadowTurns"] = new JsonArray(_shadowTurns.Select(turn => (JsonNode?)JsonValue.Create(turn)).ToArray())
	};

	protected override void LoadAdditionalState(JsonNode? state, int schemaVersion)
	{
		SetOrbs(state?["orbs"] is JsonArray orbs
			? orbs.Select(node => ModelDb.GetById<OrbModel>(ModelId.Deserialize(node!.GetValue<string>())).ToMutable()).ToArray()
			: state?["orb"]?.GetValue<string>() is { } id
				? [ModelDb.GetById<OrbModel>(ModelId.Deserialize(id)).ToMutable()] : []);
		_handFocus = state?["handFocus"]?.GetValue<int>() ?? 0;
		_focusOwner = null;
		_shadowTurns = state?["shadowTurns"] is JsonArray turns ? turns.Select(turn => turn!.GetValue<int>()).ToList() : [];
	}

	internal void ApplyInnateAffliction()
	{
		if (Owner is not { IsMutable: true } card || !Orbs.Any(orb => orb is ThunderOrb) || card.Affliction is Sealed)
			return;
		card.ClearAfflictionInternal();
		card.AfflictInternal(ModelDb.Affliction<Sealed>().ToMutable(), 1m);
	}

	internal async Task UpdateHandFocus()
	{
		if (Owner is not { IsMutable: true, Owner: { } player } card)
			return;
		var context = new ThrowingPlayerChoiceContext();
		if (_focusOwner != null && _focusOwner != player && _handFocus != 0)
		{
			int previous = _handFocus;
			_handFocus = 0;
			await PowerCmd.Apply<FocusPower>(context, _focusOwner.Creature, -previous, _focusOwner.Creature, null);
		}
		int amount = card.Pile?.Type == PileType.Hand && !card.HasBeenRemovedFromState
			? Orbs.Count(orb => orb is ForestOrb) : 0;
		int change = amount - _handFocus;
		_focusOwner = player;
		_handFocus = amount;
		if (change == 0)
			return;
		MarkDirty();
		await PowerCmd.Apply<FocusPower>(context, player.Creature, change, player.Creature, null);
	}

	public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
	{
		if (card != Owner)
			return;
		foreach (var orb in Orbs.OfType<WindOrb>().ToArray())
		{
			if (CombatManager.Instance.IsOverOrEnding || card.Owner.Creature.IsDead)
				break;
			choiceContext.PushModel(orb);
			try { await OrbCmd.Passive(choiceContext, orb, null, true); }
			finally { choiceContext.PopModel(orb); }
		}
	}

	internal void QueueShadowPlay()
	{
		if (Owner?.Owner?.PlayerCombatState is not { } state)
			return;
		_shadowTurns.Add(state.TurnNumber);
		MarkDirty();
	}

	internal async Task PlayPendingShadow(PlayerChoiceContext choiceContext, Player player)
	{
		if (Owner is not { HasBeenRemovedFromState: false } card || card.Owner != player ||
			player.PlayerCombatState is not { } state || card.Pile?.IsCombatPile != true ||
			CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead)
			return;
		int count = _shadowTurns.RemoveAll(turn => turn < state.TurnNumber);
		if (count == 0)
			return;
		MarkDirty();
		for (int i = 0; i < count; i++)
		{
			if (CombatManager.Instance.IsOverOrEnding || player.Creature.IsDead || player.PlayerCombatState != state ||
				card.HasBeenRemovedFromState || card.Pile?.IsCombatPile != true)
				break;
			await CardCmd.AutoPlay(choiceContext, card, null);
		}
	}

	public IEnumerable<CardDescriptionFragment> GetDescriptionFragments(CardDescriptionContext context)
	{
		foreach (var orb in Orbs)
		{
			string key = $"{orb.Id.Entry}.cardText";
			if (!LocString.Exists("orbs", key))
				continue;
			var text = new LocString("orbs", key);
			text.Add("Passive", orb.PassiveVal);
			text.Add("Evoke", orb.EvokeVal);
			yield return new CardDescriptionFragment(text);
		}
		if (Orbs.Any(orb => orb is ThunderOrb) && context.Card.Affliction is not Sealed)
			yield return new CardDescriptionFragment(ModelDb.Affliction<Sealed>().DynamicExtraCardText!);
	}

	public IEnumerable<IHoverTip> GetHoverTips(CardModel card)
	{
		var orbs = Orbs;
		if (orbs.Count == 0)
			return [];
		return [NagatoHoverTips.Residence, .. orbs.SelectMany(orb =>
			card.Pile?.IsCombatPile == true ? orb.HoverTips : ModelDb.GetById<OrbModel>(orb.Id).HoverTips)];
	}

	public IEnumerable<CardOverlayContribution> GetCardOverlays(CardOverlayContext context) =>
		Orbs.Any(orb => orb is ThunderOrb) && context.Card.Affliction is not Sealed
			? [CardOverlayContribution.FromScenePath("resident_thunder_sealed", NSealedCardVfx.ScenePath)] : [];

	public IEnumerable<string> GetCardOverlayAssetPaths(CardModel card) =>
		Orbs.SelectMany(orb => orb.AssetPaths)
			.Concat(Orbs.Any(orb => orb is ThunderOrb) ? [NSealedCardVfx.ScenePath] : [])
			.Concat(Orbs.Any(orb => orb is ShadowOrb) ? ModelDb.Card<ShadowReturn>().RunAssetPaths : []).Distinct();
}
