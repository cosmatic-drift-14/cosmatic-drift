using Robust.Shared.Configuration;

namespace Content.Shared._CD.CCVars;

/// <summary>
/// CD Specific Cvars
/// </summary>
[CVarDefs]
public sealed class CDCCVars
{
    /// <summary>
    /// Respawn time, how long the player has to wait in seconds after death before they can respawn.
    /// </summary>
    public static readonly CVarDef<float> RespawnTime =
        CVarDef.Create("game.respawn_time", 300.0f, CVar.SERVER | CVar.REPLICATED);
}
