using Robust.Shared.GameStates;

namespace Content.Shared._CD.Inventory;

/// <summary>
///     This is used to mark the entity as visible inside of pockets.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PocketVisibleComponent : Component;
