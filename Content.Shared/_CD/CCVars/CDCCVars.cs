using Robust.Shared.Configuration;

namespace Content.Shared._CD.CCVars;

/// <summary>
/// CD Specific Cvars
/// </summary>
[CVarDefs]
// ReSharper disable once InconsistentNaming
public sealed class CDCCVars
{
    /// <summary>
    /// Respawn time, how long the player has to wait in seconds after death before they can respawn.
    /// </summary>
    public static readonly CVarDef<float> RespawnTime =
        CVarDef.Create("game.respawn_time", 300.0f, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    /// The values in the vote for time players spend in CC are <see cref="Content.Shared.CCVar.CCVars.RoundRestartTime"/>
    /// plus and minus this value. It is strongly recommended to make this a multiple of 60.
    /// </summary>
    public static readonly CVarDef<float> RoundRestartTimeVoteDelta =
        CVarDef.Create("cd.round_restart_time_vote_delta", 300.0f, CVar.SERVER);
}
