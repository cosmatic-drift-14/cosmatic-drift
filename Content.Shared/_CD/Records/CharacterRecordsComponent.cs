using Robust.Shared.GameStates;

namespace Content.Shared._CD.Records;

/// <summary>
/// The component on the station that stores records after the round starts.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CharacterRecordsComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<uint, FullCharacterRecords> Records = new();

    [DataField, AutoNetworkedField]
    private uint _nextKey = 1;

    /// <summary>
    /// Creates a key has never been used previously
    /// </summary>
    public uint CreateNewKey()
    {
        return _nextKey++;
    }
}

public sealed record CharacterRecordKey
{
    public EntityUid Station { get; init; }
    public uint Index { get; init; }
}
