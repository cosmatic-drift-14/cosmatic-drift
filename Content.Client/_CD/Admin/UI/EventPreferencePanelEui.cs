using Content.Client.Eui;
using Content.Shared._CD.Admin;
using Content.Shared.Eui;

namespace Content.Client._CD.Admin.UI;

public sealed class EventPreferencesPanelEui : BaseEui
{
    private EventPreferencesPanel EventPreferencesPanel { get; }

    public EventPreferencesPanelEui()
    {
        EventPreferencesPanel = new EventPreferencesPanel();
    }

    public override void Opened()
    {
        EventPreferencesPanel.OpenCentered();
    }

    public override void Closed()
    {
        EventPreferencesPanel.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not EventPreferencePanelEuiState s)
            return;

        EventPreferencesPanel.SetDetails(s.Username, s.Preferences, s.VisibleAntagPrototypes);
    }

}
