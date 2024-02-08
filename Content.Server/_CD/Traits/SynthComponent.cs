using Robust.Shared.GameStates;

namespace Content.Server._CD.Traits;

/// <summary>
/// Set players' blood to coolant, and is used to notify them of ion storms
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SynthSystem))]
public sealed partial class SynthComponent : Component
{
    /// <summary>
    /// The blood reagent to give them.
    /// </summary>
    [DataField("newBloodReagent")]
    public string NewBloodReagent = "SynthBlood";
}
