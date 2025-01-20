using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Silicons.StationAi;

/// <summary>
/// Given to a brain to allow an AI to take it over.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StationAiShellBrainComponent : Component
{
    /// <summary>
    /// If this brain is currently possessed
    /// </summary>
    [ViewVariables]
    public bool Active;

    /// <summary>
    /// The core which is possessing this brain
    /// </summary>
    [ViewVariables]
    public EntityUid? ActiveCore;
}
