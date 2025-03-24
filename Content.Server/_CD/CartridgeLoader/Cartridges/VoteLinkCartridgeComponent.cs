namespace Content.Server._CD.CartridgeLoader.Cartridges;

[RegisterComponent, Access(typeof(VoteLinkCartridgeSystem))]
public sealed partial class VoteLinkCartridgeComponent : Component
{
    /// <summary>
    /// Station entity to keep track of.
    /// </summary>
    [DataField]
    public EntityUid? Station;
}
