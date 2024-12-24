using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.JobSlotsConsole;

[RegisterComponent]
public sealed partial class JobSlotsConsoleComponent : Component
{
    /// <summary>
    /// The station this console is linked to. Set when the station is detected.
    /// </summary>
    [DataField]
    public EntityUid? Station;

    /// <summary>
    /// Jobs that cannot have their slots adjusted from this console.
    /// </summary>
    [DataField]
    public HashSet<ProtoId<JobPrototype>> Blacklist = [];

    /// <summary>
    /// Whether this console has debug features enabled, like toggling infinite slots.
    /// </summary>
    [DataField]
    public bool Debug;

    /// <summary>
    /// The sound to play if the player doesn't have access to change job slots.
    /// </summary>
    [DataField]
    public SoundSpecifier DenySound = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_sigh.ogg");
}
