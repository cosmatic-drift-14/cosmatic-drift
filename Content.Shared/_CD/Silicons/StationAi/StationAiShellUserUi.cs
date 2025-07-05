using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Silicons.StationAi;

[Serializable, NetSerializable]
public sealed class JumpToShellMessage(NetEntity? shell) : BoundUserInterfaceMessage
{
    public readonly NetEntity? Shell = shell;
}

[Serializable, NetSerializable]
public sealed class EnterShellMessage(NetEntity? shell) : BoundUserInterfaceMessage
{
    public readonly NetEntity? Shell = shell;
}

[Serializable, NetSerializable]
public sealed class SelectShellMessage(NetEntity? shell) : BoundUserInterfaceMessage
{
    public readonly NetEntity? Shell = shell;
}

[Serializable, NetSerializable]
public enum ShellUiKey : byte
{
    Key
}
