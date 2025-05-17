using Content.Shared.Eui;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Admin;

[Serializable, NetSerializable]
public sealed class CharacterPanelEuiState(
    NetUserId userID,
    string player,
    string? description,
    HumanoidCharacterProfile preferences,
    List<AntagPrototype> visibleAntagPrototypes
    ) : EuiStateBase
{
    public readonly NetUserId UserID = userID;
    public readonly string Player = player;
    public readonly string? CharacterDescription = description;

    public readonly HumanoidCharacterProfile Preferences = preferences;
    public readonly List<AntagPrototype> VisibleAntagPrototypes = visibleAntagPrototypes;
}
