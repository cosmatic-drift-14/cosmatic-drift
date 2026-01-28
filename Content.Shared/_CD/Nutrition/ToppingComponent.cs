using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Nutrition.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ToppingComponent : Component
{
    /// <summary>
    /// Volume of solution to transfer, if null transfer all
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2? PortionSize = null;

    /// <summary>
    /// ID of solution container to add from
    /// </summary>
    [DataField, AutoNetworkedField]
    public string Solution;
}
