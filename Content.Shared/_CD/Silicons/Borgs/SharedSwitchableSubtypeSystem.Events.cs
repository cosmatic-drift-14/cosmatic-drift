using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Silicons.Borgs;

[Serializable, NetSerializable]
public sealed class BorgSelectSubtypeMessage(ProtoId<BorgSubtypePrototype>? subtype) : BoundUserInterfaceMessage
{
    public ProtoId<BorgSubtypePrototype>? Subtype = subtype;
}

[ByRefEvent]
public record struct AfterBorgTypeSelectEvent;
