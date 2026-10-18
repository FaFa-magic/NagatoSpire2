using MegaCrit.Sts2.Core.Models;
using NagatoSpire2.NagatoSpire2Code.Orbs;

namespace NagatoSpire2.NagatoSpire2Code.Cards.Token;

public sealed class ShellSelectionFire : ShellSelectionOption
{
	protected override OrbModel Orb => ModelDb.Orb<FireOrb>();
}
