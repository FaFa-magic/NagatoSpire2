using System.Text.Json.Nodes;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;

namespace NagatoSpire2.NagatoSpire2Code.Capabilities;

[RegisterModelCapability]
public sealed class LoadedCardCapability : CardCapability
{
	public int SlotIndex { get; private set; }

	internal void SetSlot(int index)
	{
		AssertMutable();
		if (SlotIndex == index)
			return;
		SlotIndex = index;
		MarkDirty();
	}

	protected override JsonNode? SaveAdditionalState() => SlotIndex > 0 ? new JsonObject { ["slot"] = SlotIndex } : null;

	protected override void LoadAdditionalState(JsonNode? state, int schemaVersion) => SlotIndex = state?["slot"]?.GetValue<int>() ?? 0;

	public override IModelCapability CloneFor(AbstractModel clonedOwner)
	{
		var clone = (LoadedCardCapability)base.CloneFor(clonedOwner);
		clone.SlotIndex = 0;
		return clone;
	}
}
