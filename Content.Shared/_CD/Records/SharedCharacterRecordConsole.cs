using Content.Shared.Security;
using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Records;

[Serializable, NetSerializable]
public enum CharacterRecordConsoleKey : byte
{
    Key
}

[Serializable, NetSerializable]
public enum RecordConsoleType : byte
{
    Security, Medical, Employment, Admin
}

[Serializable, NetSerializable]
public sealed class CharacterRecordConsoleState : BoundUserInterfaceState
{
    public RecordConsoleType ConsoleType { get; set; }

    public NetEntity? Selected { get; set; } = null;

    public Dictionary<NetEntity, (string, uint?)>? RecordListing { get; set; }

    public FullCharacterRecords? SelectedRecord { get; set; } = null;

    public StationRecordsFilter? Filter { get; set; } = null;

    public (SecurityStatus, string?)? SecurityStatus = null;
}

[Serializable, NetSerializable]
public sealed class CharacterRecordsConsoleFilterMsg : BoundUserInterfaceMessage
{
    public readonly StationRecordsFilter? Filter;

    public CharacterRecordsConsoleFilterMsg(StationRecordsFilter? filter)
    {
        Filter = filter;
    }
}

[Serializable, NetSerializable]
public sealed class CharacterRecordConsoleSelectMsg : BoundUserInterfaceMessage
{
    public readonly NetEntity? Key;

    public CharacterRecordConsoleSelectMsg(NetEntity? key)
    {
        Key = key;
    }
}
