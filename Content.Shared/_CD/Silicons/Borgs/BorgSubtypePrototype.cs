using Content.Shared.Silicons.Borgs;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Silicons.Borgs;
/// <summary>
///  Information relating to a borg's subtype. Should be purely cosmetic.
/// </summary>
[Prototype, Serializable, NetSerializable]
public sealed partial class BorgSubtypePrototype : IPrototype
{
    [IdDataField]
    public required string ID { get; set; }

    /// <summary>
    /// Prototype to display in the selection menu for the subtype.
    /// </summary>
    [DataField]
    public required EntProtoId DummyPrototype;

    /// <summary>
    /// The parent borg type that the subtype will be shown under in the selection menu.
    /// </summary>
    [DataField]
    public required ProtoId<BorgTypePrototype> ParentBorgType = "generic";
}
