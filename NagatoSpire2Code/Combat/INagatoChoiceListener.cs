using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace NagatoSpire2.NagatoSpire2Code.Combat;

public interface INagatoChoiceListener
{
	Task AfterNagatoChoice(PlayerChoiceContext choiceContext);
}
