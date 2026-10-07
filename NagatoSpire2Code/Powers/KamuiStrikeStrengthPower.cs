using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using NagatoSpire2.NagatoSpire2Code.Cards.Common;

namespace NagatoSpire2.NagatoSpire2Code.Powers;

public sealed class KamuiStrikeStrengthPower : NagatoTempPowerModel<KamuiStrike, StrengthPower>
{
	public override LocString Title => new("powers", "NAGATO_SPIRE2_POWER_KAMUI_STRIKE_STRENGTH_POWER.title");
}
