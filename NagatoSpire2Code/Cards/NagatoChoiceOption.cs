using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using NagatoSpire2.NagatoSpire2Code.Characters;
using STS2RitsuLib.Interop.AutoRegistration;

namespace NagatoSpire2.NagatoSpire2Code.Cards;

[RegisterCard(typeof(TokenCardPool), Inherit = true)]
public abstract class NagatoChoiceOption() : NagatoCardModel(-1, CardType.Skill, CardRarity.Token, TargetType.None, false)
{
	public override bool CanBeGeneratedInCombat => false;

	public override CardPoolModel VisualCardPool => ModelDb.CardPool<NagatoCardPool>();

	public abstract Task OnChosen(PlayerChoiceContext choiceContext);
}
