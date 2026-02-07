using Robust.Shared.GameStates;

namespace Content.Shared._CD;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]

public sealed partial class ExtendedEquipmentComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Slot = "default";

    [DataField, AutoNetworkedField]
    public EntityUid ParentEquipment;
}
