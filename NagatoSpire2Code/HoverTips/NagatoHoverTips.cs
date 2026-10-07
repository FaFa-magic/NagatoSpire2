using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace NagatoSpire2.NagatoSpire2Code.HoverTips;

public static class NagatoHoverTips
{
	public static IHoverTip Load => Create("LOAD");
	public static IHoverTip TemporaryPower => Create("TEMPORARY_POWER");

	private static IHoverTip Create(string name) => new HoverTip(
		new LocString("static_hover_tips", $"NAGATO_SPIRE2_{name}.title"),
		new LocString("static_hover_tips", $"NAGATO_SPIRE2_{name}.description"));
}
