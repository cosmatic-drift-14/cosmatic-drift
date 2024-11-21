using Content.Client.Administration.Managers;
using Content.Client.Eui;

namespace Content.Client._CD.Admin.UI;

public sealed class EventPreferenceEui : BaseEui
{
    [Dependency] private readonly IClientAdminManager _admin = default!;

    private EventPreferences EventPreferencesPanel { get; }

    public EventPreferenceEui()
    {
        EventPreferencesPanel = new EventPreferences();
    }
}
