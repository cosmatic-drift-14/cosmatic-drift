using Content.Client.Administration.Managers;
using Content.Client.Eui;
using Microsoft.CodeAnalysis.Elfie.Serialization;

namespace Content.Client._CD.Admin.UI;

public sealed class EventPreferencePanelEui : BaseEui
{
    [Dependency] private readonly IClientAdminManager _admin = default!;

    private EventPreferences EventPreferencesPanel { get; }

    public EventPreferencePanelEui()
    {
        EventPreferencesPanel = new EventPreferences(_admin);
    }
}
