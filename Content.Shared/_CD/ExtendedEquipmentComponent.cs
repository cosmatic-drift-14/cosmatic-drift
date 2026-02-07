using Robust.Shared.GameStates;

namespace Content.Shared._CD;

/// <summary>
/// Component signifying that this entity is 'extending' from an entity
/// with the <see cref="ExtendableClothingComponent"/> component.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(ExtendableClothingSystem))]
public sealed partial class ExtendedEquipmentComponent : Component
{
    /// <summary>
    /// Inventory slot we'll want this equipment to be equipped to.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string Slot = "default";

    [DataField, AutoNetworkedField]
    public EntityUid ParentEquipment;
}
