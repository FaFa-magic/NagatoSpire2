using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Cards;

public abstract class NagatoLoadChoiceOption<TOrb> : NagatoChoiceOption where TOrb : OrbModel
{
	public int? SlotIndex { get; set; }

	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
	[
		NagatoHoverTips.Load,
		HoverTipFactory.FromOrb<TOrb>()
	];

	public override Task OnChosen(PlayerChoiceContext choiceContext) =>
		NagatoOrbCmd.Load(choiceContext, Owner, ModelDb.Orb<TOrb>().ToMutable(), SlotIndex);

	protected override void OnUpgrade() { }
}
