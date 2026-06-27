using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CD;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ExtendableClothingComponent : Component
{
    /// <summary>
    /// The id for the container when all the equipment is retracted
    /// </summary>
    [ViewVariables]
    public const string EquipmentContainerId = "equipment_container";

    /// <summary>
    /// All the equipment we want to spawn and attach to this parent clothing when initialized
    /// </summary>
    [DataField]
    public Dictionary<string, EntProtoId> AttachedEquipment { get; set; } = new();

    /// <summary>
    /// All equipment that is currently equipped by an entity that is wearing this parent.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntityUid> CurrentlyEquipped = new();

}
