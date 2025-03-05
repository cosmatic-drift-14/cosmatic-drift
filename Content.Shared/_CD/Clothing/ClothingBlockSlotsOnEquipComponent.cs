using Robust.Shared.GameStates;

namespace Content.Shared._CD.Clothing;

/// <summary>
/// This component prevents equipping items in specified slots when this clothing item is worn,
/// and prevents equipping this item if any of the specified slots are occupied.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(ClothingBlockSlotsOnEquipSystem))]
public sealed partial class ClothingBlockSlotsOnEquipComponent : Component
{
    /// <summary>
    /// The slots that will be blocked when this clothing item is equipped
    /// </summary>
    [DataField]
    public HashSet<string> BlockedSlots = [];
}
