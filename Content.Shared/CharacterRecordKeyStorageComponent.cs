using Content.Shared._CD.Records;
using Robust.Shared.GameStates;

namespace Content.Shared;

/// <summary>
/// Stores the key to the entities character records.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CharacterRecordKeyStorageComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public CharacterRecordKey Key;

    public CharacterRecordKeyStorageComponent(CharacterRecordKey key)
    {
        Key = key;
    }
}
