using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Capabilities;

[RegisterModelCapability]
public sealed class CardOrbCapability : ModelCapability<CardModel>, ICardDescriptionContributor, ICardHoverTipContributor,
	ICardOverlayAssetPathContributor
{
	public const int MaxOrbs = 3;
	private IReadOnlyList<OrbModel> _orbs = [];

	public IReadOnlyList<OrbModel> Orbs
	{
		get
		{
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
		return clone;
	}

	protected override JsonNode? SaveAdditionalState() => _orbs.Count == 0 ? null : new JsonObject
	{
		["orbs"] = new JsonArray(_orbs.Select(orb => (JsonNode?)JsonValue.Create(orb.Id.ToString())).ToArray())
	};

	protected override void LoadAdditionalState(JsonNode? state, int schemaVersion) =>
		SetOrbs(state?["orbs"] is JsonArray orbs
			? orbs.Select(node => ModelDb.GetById<OrbModel>(ModelId.Deserialize(node!.GetValue<string>())).ToMutable()).ToArray()
			: state?["orb"]?.GetValue<string>() is { } id
				? [ModelDb.GetById<OrbModel>(ModelId.Deserialize(id)).ToMutable()] : []);

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
	}

	public IEnumerable<IHoverTip> GetHoverTips(CardModel card)
	{
		var orbs = Orbs;
		if (orbs.Count == 0)
			return [];
		return [NagatoHoverTips.Residence, .. orbs.SelectMany(orb =>
			card.Pile?.IsCombatPile == true ? orb.HoverTips : ModelDb.GetById<OrbModel>(orb.Id).HoverTips)];
	}

	public IEnumerable<string> GetCardOverlayAssetPaths(CardModel card) => Orbs.SelectMany(orb => orb.AssetPaths).Distinct();
}
