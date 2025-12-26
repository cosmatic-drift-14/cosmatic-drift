using Content.Shared.FixedPoint;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._CD.Species;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedMouthStorageSystem))]
public sealed partial class MouthStorageComponent : Component
{
    public const string MouthContainerId = "mouth";

    [DataField]
    public EntProtoId? OpenStorageAction = "ActionOpenMouthStorage";

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Action;

    [DataField]
    public EntProtoId MouthProto = "CheekStorage";

    /// <summary>
    /// Where the mouth is stored
    /// </summary>
    [ViewVariables]
    public Container Mouth = default!;

    /// <summary>
    /// The mouth entity
    /// </summary>
    [ViewVariables]
    public EntityUid? MouthId;

    // Mimimum inflicted damage on hit to spit out items
    [DataField]
    public FixedPoint2 SpitDamageThreshold = FixedPoint2.New(2);
}
