using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using NagatoSpire2.NagatoSpire2Code.Cards.Token;
using NagatoSpire2.NagatoSpire2Code.Combat;
using NagatoSpire2.NagatoSpire2Code.HoverTips;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class BigSevenPower : NagatoPowerModel
{
	private sealed class Data
	{
		public readonly Dictionary<CardPlay, int> Plays = [];
	}

	public override PowerType Type => PowerType.Buff;
	public override PowerStackType StackType => PowerStackType.Counter;
	protected override IEnumerable<IHoverTip> AdditionalHoverTips => [NagatoHoverTips.Loading, NagatoHoverTips.Salvo, HoverTipFactory.FromCard<Shell>()];
	protected override object InitInternalData() => new Data();

	public override Task BeforeCardPlayed(CardPlay cardPlay)
	{
		if (cardPlay.Player == Owner.Player && !(cardPlay.Card is Shell && NagatoCardSlots.IsSalvo(cardPlay.Card)))
			GetInternalData<Data>().Plays[cardPlay] = Amount;
		return Task.CompletedTask;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (!GetInternalData<Data>().Plays.Remove(cardPlay, out int count) || count <= 0 ||
			Owner.Player?.PlayerCombatState is not { } state || Owner.CombatState is not { } combat ||
			Owner.IsDead || CombatManager.Instance.IsOverOrEnding)
			return;
		Flash();
		for (int i = 0; i < count; i++)
		{
			if (Owner.IsDead || CombatManager.Instance.IsOverOrEnding || Owner.Player.PlayerCombatState != state)
				break;
			var shell = combat.CreateCard<Shell>(Owner.Player);
			await CardPileCmd.AddGeneratedCardToCombat(shell, PileType.Play, Owner.Player);
			await NagatoCardSlots.Load(choiceContext, shell);
		}
	}
}
