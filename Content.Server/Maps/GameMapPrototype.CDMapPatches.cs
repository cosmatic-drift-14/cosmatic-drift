using Robust.Shared.Utility;

namespace Content.Server.Maps;

public sealed partial class GameMapPrototype
{
    /// <summary>
    ///     Provides a list of patches to make to the map.
    /// </summary>
    [DataField]
    public ResPath? Patchfile = null;
}
