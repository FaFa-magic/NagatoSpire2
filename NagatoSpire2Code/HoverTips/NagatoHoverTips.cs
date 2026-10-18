using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace NagatoSpire2.NagatoSpire2Code.HoverTips;

public static class NagatoHoverTips
{
	public static IHoverTip TemporaryPower => Create("TEMPORARY_POWER");
	public static IHoverTip Residence => Create("RESIDENCE");
	public static IHoverTip Loading => Create("LOADING");
	public static IHoverTip Salvo => Create("SALVO");

	private static IHoverTip Create(string name) => new HoverTip(
		new LocString("static_hover_tips", $"NAGATO_SPIRE2_{name}.title"),
		new LocString("static_hover_tips", $"NAGATO_SPIRE2_{name}.description"));
}
