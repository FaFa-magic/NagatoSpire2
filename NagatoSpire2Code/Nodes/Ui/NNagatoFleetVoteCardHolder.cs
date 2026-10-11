using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;

namespace NagatoSpire2.NagatoSpire2Code.Nodes.Ui;

public partial class NNagatoFleetVoteCardHolder : NCardHolder
{
	public const string ScenePath = "res://NagatoSpire2/scenes/ui/fleet_vote_card_holder.tscn";
	public const string CardScenePath = "res://scenes/cards/card.tscn";

	public static NNagatoFleetVoteCardHolder Create(CardModel model)
	{
		var holder = PreloadManager.Cache.GetScene(ScenePath).Instantiate<NNagatoFleetVoteCardHolder>();
		var card = PreloadManager.Cache.GetScene(CardScenePath).Instantiate<NCard>();
		card.Model = model;
		holder.SetCard(card);
		holder.Scale = holder.SmallScale;
		return holder;
	}

	public override void _Ready()
	{
		ConnectSignals();
		CardNode!.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
	}
}
