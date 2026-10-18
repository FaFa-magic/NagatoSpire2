using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Commands;
using NagatoSpire2.NagatoSpire2Code.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Keywords;
using NagatoSpire2.NagatoSpire2Code.Powers;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Uncommon;

public sealed class DivineWill() : NagatoCardModel(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
	public override IEnumerable<CardKeyword> CanonicalKeywords => [NagatoKeywords.Choice];
	protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<DivineWillPlatingPower>(3m)];
	protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
		[NagatoHoverTips.Loading, HoverTipFactory.FromCard<Shell>(IsUpgraded),
			HoverTipFactory.FromPower<PlatingPower>()];
	protected override IEnumerable<string> ExtraRunAssetPaths =>
		[.. base.ExtraRunAssetPaths, .. ModelDb.Card<DivineWillLoading>().RunAssetPaths, .. ModelDb.Card<DivineWillPlating>().RunAssetPaths];

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (CombatState is not { } combat || Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		CardModel[] options = [combat.CreateCard<DivineWillLoading>(Owner), combat.CreateCard<DivineWillPlating>(Owner)];
		if (IsUpgraded)
			foreach (var option in options)
				CardCmd.Upgrade(option);
		await NagatoChoiceCmd.Choose(choiceContext, Owner, options);
	}

	protected override void OnUpgrade() => DynamicVars["DivineWillPlatingPower"].UpgradeValueBy(1m);
}
