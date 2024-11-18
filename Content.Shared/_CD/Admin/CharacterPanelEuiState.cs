using Content.Shared.Eui;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Admin;

[Serializable, NetSerializable]
public sealed class CharacterPanelEuiState(NetUserId userID,string player, string description) : EuiStateBase
{
    public readonly NetUserId UserID = userID;
    public readonly string Player = player;
    public readonly string CharacterDescription = description;
}
