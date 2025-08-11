using Content.Shared.Radio;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Silicons.StationAi;

/// <summary>
/// Given to an AI core to allow it to take over shells
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
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
    [DataField]
    [AutoNetworkedField]
    public EntityUid? SelectedShell;

    /// <summary>
    /// The EntityUid of the brain in the selected shell
    /// </summary>
    [DataField]
    public EntityUid? SelectedBrain;

    /// <summary>
    /// The current lawset of the AI controlling the shell
    /// </summary>
    [DataField]
    public SiliconLawset? ControllingAiLaws;

    /// <summary>
    /// The radio channels added by controlling a shell to the ActiveRadioComponent
    /// Used to keep track of all the channels not inherent to a shell
    /// </summary>
    [DataField]
    public HashSet<ProtoId<RadioChannelPrototype>> ActiveAddedChannels = new();

    /// <summary>
    /// The radio channels added by controlling a shell to the IntrinsicRadioTransmitterComponent
    /// Used to keep track of all the channels not inherent to a shell
    /// </summary>
    [DataField]
    public HashSet<ProtoId<RadioChannelPrototype>> TransmitterAddedChannels = new();

    /// <summary>
    /// All the shells that is available for this AI to control
    /// Updated whenever a BORIS module is inserted or ejected from a chassis
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public List<EntityUid> ControllableShells = new();
}
