using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Nutrition.Components;

[RegisterComponent]
public sealed partial class ToppingComponent : Component
{
    /// <summary>
    /// Volume of solution to transfer, if null transfer all
    /// </summary>
    [DataField]
    public FixedPoint2? PortionSize = null;

    /// <summary>
    /// ID of solution container to add from
    /// </summary>
    [DataField]
    public string Solution;
}
