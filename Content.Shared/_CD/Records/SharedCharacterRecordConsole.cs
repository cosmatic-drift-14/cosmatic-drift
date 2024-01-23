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

    public Dictionary<NetEntity, string>? RecordListing { get; set; }

    public FullCharacterRecords? SelectedRecord { get; set; } = null;

    public GeneralStationRecordsFilter? Filter { get; set; } = null;
}

[Serializable, NetSerializable]
public sealed class CharacterRecordsConsoleFilterMsg : BoundUserInterfaceMessage
{
    public readonly GeneralStationRecordsFilter? Filter;

    public CharacterRecordsConsoleFilterMsg(GeneralStationRecordsFilter? filter)
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
