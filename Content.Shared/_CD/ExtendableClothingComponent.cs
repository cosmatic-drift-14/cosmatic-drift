using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CD;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ExtendableClothingComponent : Component
{
    /// <summary>
    /// The id for the container when all the equipment is retracted
    /// </summary>
    [ViewVariables]
    public const string EquipmentContainerId = "equipment_container";

    [DataField]
    public Dictionary<string, EntProtoId> AttachedEquipment { get; set; } = new();

    [DataField, AutoNetworkedField]
    public List<EntityUid> CurrentlyEquipped = new();

}
