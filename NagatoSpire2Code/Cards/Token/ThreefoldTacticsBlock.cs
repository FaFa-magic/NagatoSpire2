using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class ThreefoldTacticsBlock : NagatoChoiceOption
{
	public CardPlay SourcePlay { get; set; } = null!;

	public override bool GainsBlock => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5m, ValueProp.Move)];

	public override Task OnChosen(PlayerChoiceContext choiceContext) =>
		CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, SourcePlay);

	protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(1m);
}
