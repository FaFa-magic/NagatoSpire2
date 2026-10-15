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
	private OrbModel? _orb;

	public OrbModel? Orb
	{
		get
		{
			if (_orb != null && Owner is { IsMutable: true, Owner: { } player } && _orb.Owner != player)
			{
				_orb.Owner = null!;
				_orb.Owner = player;
			}
			return _orb;
		}
	}

	public void SetOrb(OrbModel? orb)
	{
		AssertMutable();
		orb?.AssertMutable();
		_orb = orb;
		if (Owner != null)
			MarkDirty();
	}

	public override IModelCapability CloneFor(AbstractModel clonedOwner)
	{
		var clone = (CardOrbCapability)base.CloneFor(clonedOwner);
		if (_orb != null)
		{
			clone._orb = (OrbModel)_orb.MutableClone();
			clone._orb.Owner = null!;
		}
		return clone;
	}

	protected override JsonNode? SaveAdditionalState() => _orb == null ? null : new JsonObject
	{
		["orb"] = _orb.Id.ToString()
	};

	protected override void LoadAdditionalState(JsonNode? state, int schemaVersion) =>
		_orb = state?["orb"]?.GetValue<string>() is { } id
			? ModelDb.GetById<OrbModel>(ModelId.Deserialize(id)).ToMutable() : null;

	public IEnumerable<CardDescriptionFragment> GetDescriptionFragments(CardDescriptionContext context)
	{
		if (Orb is not { } orb)
			yield break;
		string key = $"{orb.Id.Entry}.cardText";
		if (!LocString.Exists("orbs", key))
			yield break;
		var text = new LocString("orbs", key);
		text.Add("Passive", orb.PassiveVal);
		text.Add("Evoke", orb.EvokeVal);
		yield return new CardDescriptionFragment(text);
	}

	public IEnumerable<IHoverTip> GetHoverTips(CardModel card)
	{
		if (Orb is not { } orb)
			return [];
		return [NagatoHoverTips.Residence, .. card.Pile?.IsCombatPile == true ? orb.HoverTips : [orb.DumbHoverTip]];
	}

	public IEnumerable<string> GetCardOverlayAssetPaths(CardModel card) => Orb?.AssetPaths ?? [];
}
