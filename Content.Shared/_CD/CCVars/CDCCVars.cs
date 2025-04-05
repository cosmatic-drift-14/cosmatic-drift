using Robust.Shared.Configuration;

namespace Content.Shared._CD.CCVars;

/// <summary>
/// CD Specific Cvars
/// </summary>
[CVarDefs]
public sealed class CDCCVars
{
    /// <summary>
    /// Depth of the recent map cache. Prevents recent maps from repeating in map votes.
    /// </summary>
    public static readonly CVarDef<int> MapvoteCacheDepth =
        CVarDef.Create("cd.game.mapvote_cache_depth", 2, CVar.SERVER);

    /// <summary>
    /// Respawn time, how long the player has to wait in seconds after death before they can respawn.
    /// </summary>
    public static readonly CVarDef<float> RespawnTime =
        CVarDef.Create("cd.game.respawn_time", 300.0f, CVar.SERVER | CVar.REPLICATED);
}
