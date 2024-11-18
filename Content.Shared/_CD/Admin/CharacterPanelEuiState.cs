using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Admin;

[Serializable, NetSerializable]
public sealed class CharacterPanelEuiState(string player) : EuiStateBase
{
    public readonly string Player = player;
}
