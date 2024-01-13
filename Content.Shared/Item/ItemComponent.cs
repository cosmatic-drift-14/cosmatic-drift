using Content.Shared.Hands.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Item;

/// <summary>
///     Handles items which can be picked up to hands and placed in pockets, as well as storage containers
///     like backpacks.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
[Access(typeof(SharedItemSystem)), AutoGenerateComponentState(true)]
public sealed partial class ItemComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    [Access(typeof(SharedItemSystem))]
    public int Size = 5;

    [Access(typeof(SharedItemSystem))]
    [DataField("inhandVisuals")]
    public Dictionary<HandLocation, List<PrototypeLayerData>> InhandVisuals = new();

    [Access(typeof(SharedItemSystem))]
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField, AutoNetworkedField]
    public string? HeldPrefix;

    /// <summary>
    ///     Rsi of the sprite shown on the player when this item is in their hands. Used to generate a default entry for <see cref="InhandVisuals"/>
    /// </summary>
    [Access(typeof(SharedItemSystem))]
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("sprite")]
    public string? RsiPath;
}

/// <summary>
///     Raised when an item's visual state is changed. The event is directed at the entity that contains this item, so
///     that it can properly update its hands or inventory sprites and GUI.
/// </summary>
[Serializable, NetSerializable]
public sealed class VisualsChangedEvent : EntityEventArgs
{
    public readonly NetEntity Item;
    public readonly string ContainerId;

    public VisualsChangedEvent(NetEntity item, string containerId)
    {
        Item = item;
        ContainerId = containerId;
    }
}

/// <summary>
///     Reference sizes for common containers and items.
/// </summary>
public enum ReferenceSizes
{
    Wallet = 4,
    Pocket = 12,
    Box = 24,
    Belt = 30,
    Toolbox = 60,
    Backpack = 100,
    NoStoring = 9999
}
