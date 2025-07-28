namespace Content.Server._CD.Loadouts;

/// <summary>
/// Marker that should be attached to the PDA to rename the contained ID to the user's requested job. Used to implement custom tile loadouts.
/// </summary>
[RegisterComponent]
public sealed partial class RenameIdComponent : Component
{
    [DataField(required: true)]
    public string Value;

    [DataField]
    public string? NewIcon;
}
