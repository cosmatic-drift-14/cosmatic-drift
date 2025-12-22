using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.CryoSleep;

[RegisterComponent, NetworkedComponent]
public sealed partial class LostAndFoundComponent : Component
{
    /// <summary>
    /// Dictionary mapping player names to their stored items
    /// </summary>
    [DataField]
    public Dictionary<string, List<LostItemData>> StoredItems = new();
}

[Serializable, NetSerializable]
public sealed class LostItemData(string slotName, string itemName, NetEntity itemEntity)
{
    public string SlotName = slotName;
    public string ItemName = itemName;
    public NetEntity ItemEntity = itemEntity;
}
