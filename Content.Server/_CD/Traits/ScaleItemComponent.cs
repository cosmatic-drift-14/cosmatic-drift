using Robust.Shared.GameStates;

namespace Content.Server._CD.Traits;

/// <summary>
/// This will make someone an item with size, optionally checking for max or min scale.
/// Will probably be necessary to easily make someone an item whenever gridinv comes
/// </summary>
[RegisterComponent]
public sealed partial class ScaleItemComponent : Component
{
    /// <summary>
    /// What item size the person should be mkmade
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int Size = 120;

    /// <summary>
    /// The maximum scale their player can be set to for this component to itemize them
    /// If null, no max scale
    /// This uses humanoidappearancecomponent and thus will break if they're manually scaled via command.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float? MaxScale;

    /// <summary>
    /// The minimum scale their player can be set to for this component to itemize them
    /// Same caveats as MaxScale.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float? MinScale;

    /// <summary>
    /// If the player should be alerted for any of the conditions failing.
    /// Will do nothing if there are no conditions.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool AlertPlayer = true;
}

