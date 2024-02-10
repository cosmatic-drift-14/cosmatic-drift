using Robust.Shared.Configuration;

namespace Content.Shared._CD;

[CVarDefs]
public sealed class CDCvars
{
    /// <summary>
    /// The hash of the last seen welcome popup
    /// </summary>
    public static readonly CVarDef<int> WelcomePopupLastSeen =
        CVarDef.Create("cd.welcome_last_seen", 0, CVar.ARCHIVE | CVar.CLIENTONLY);
}
