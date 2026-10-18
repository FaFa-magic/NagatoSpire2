using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Keywords;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class DivineWillPower : NagatoPowerModel, INagatoChoiceListener
{
	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override bool UsesPackedIcon => true;
	protected override IEnumerable<DynamicVar> CanonicalVars => [new IntVar("UpgradedShells", 0)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[HoverTipFactory.FromKeyword(NagatoKeywords.Choice), NagatoHoverTips.Loading, HoverTipFactory.FromCard<Shell>()];

	[SavedProperty]
	public int UpgradedShells
	{
		get => DynamicVars["UpgradedShells"].IntValue;
		set
		{
			AssertMutable();
			DynamicVars["UpgradedShells"].BaseValue = Math.Max(value, 0);
			InvokeDisplayAmountChanged();
		}
	}

	public async Task AfterNagatoChoice(PlayerChoiceContext choiceContext)
	{
		if (Owner.Player?.PlayerCombatState is not { } state || Owner.CombatState is not { } combat ||
			Owner.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		int count = Amount;
		int upgraded = Math.Min(UpgradedShells, count);
		Flash();
		for (int i = 0; i < count; i++)
		{
			if (Owner.IsDead || CombatManager.Instance.IsOverOrEnding || Owner.Player.PlayerCombatState != state)
				break;
			var shell = combat.CreateCard<Shell>(Owner.Player);
			if (i < upgraded)
			{
				shell.UpgradeInternal();
				shell.FinalizeUpgradeInternal();
			}
			await CardPileCmd.AddGeneratedCardToCombat(shell, PileType.Play, Owner.Player);
			await NagatoCardSlots.Load(choiceContext, shell);
		}
	}
}
