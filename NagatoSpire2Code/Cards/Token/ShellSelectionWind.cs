using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class ShellSelectionWind : ShellSelectionOption
{
	protected override OrbModel Orb => ModelDb.Orb<WindOrb>();
}
