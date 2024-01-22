using Content.Shared._CD.Records;

namespace Content.Server._CD.Records;

[RegisterComponent]
[Access(typeof(CharacterRecordsSystem))]
public sealed partial class CharacterRecordsComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<EntityUid, CharacterRecords> Records = new();
}
