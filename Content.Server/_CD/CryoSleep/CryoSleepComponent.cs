using Robust.Shared.Audio;
using Robust.Shared.Containers;
using System.Numerics;

namespace Content.Server._CD.CryoSleep;

[RegisterComponent, Access(typeof(CryoSleepSystem))]
public sealed partial class CryoSleepComponent : Component
{
    public ContainerSlot BodyContainer = default!;

    /// <summary>
    /// Whether or not spawns are routed through the cryopod.
    /// </summary>
    [DataField]
    public bool DoSpawns;

    /// <summary>
    /// The sound that is played when a player spawns in the pod.
    /// </summary>
    [DataField]
    public SoundSpecifier ArrivalSound = new SoundPathSpecifier("/Audio/Effects/teleport_arrival.ogg");

    /// <summary>
    /// How long the entity initially is asleep for upon joining.
    /// </summary>
    [DataField]
    public Vector2 InitialSleepDurationRange = new (5, 10);
}
