﻿using Content.Shared.Eui;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Admin;

[Serializable, NetSerializable]
public sealed class EventPreferencePanelEuiState(
    string username,
    HumanoidCharacterProfile preferences,
    List<AntagPrototype> visibleAntagPrototypes) : EuiStateBase
{
    public readonly string Username = username;
    public readonly HumanoidCharacterProfile Preferences = preferences;
    public readonly List<AntagPrototype> VisibleAntagPrototypes = visibleAntagPrototypes;
}
