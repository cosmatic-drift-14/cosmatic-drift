using Content.Server.Body.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server.Body.Components;

/// <summary>
/// Causes an allergic reaction when exposed to a reagent.
/// </summary>
[RegisterComponent] [Access(typeof(AllergySystem))]
public sealed partial class AllergyComponent : Component
{
    /// <summary>
    /// The next time that reagents will be metabolized.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextUpdate;

    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, FixedPoint2> Reagents = new();

    /// <summary>
    /// How often to metabolize reagents.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(1);
}
