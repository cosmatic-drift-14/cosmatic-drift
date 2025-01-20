using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Silicons.StationAi;

/// <summary>
/// Given to an AI core to allow it to take over shells
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class StationAiShellUserComponent : Component
{
    [DataField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// The action to use for transferring back to the shell
    /// </summary>
    [DataField("actionPrototype", required: true)]
    public EntProtoId ActionPrototype;

    /// <summary>
    /// The selected shell's EntityUid
    /// </summary>
    [ViewVariables]
    public EntityUid? SelectedShell;

    /// <summary>
    /// The EntityUid of the brain in the selected shell
    /// </summary>
    [ViewVariables]
    public EntityUid? SelectedBrain;
}

[Serializable, NetSerializable]
public sealed class JumpToShellMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class EnterShellMessage : BoundUserInterfaceMessage
{
}
