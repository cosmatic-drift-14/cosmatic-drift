using Content.Client.Eui;

namespace Content.Client._CD.Admin.UI.CharacterPanel;

public sealed class CharacterPanelEui : BaseEui
{
    private CharacterPanel CharacterPanel { get; }

    public CharacterPanelEui()
    {
        CharacterPanel = new CharacterPanel();
    }

    public override void Opened()
    {
        CharacterPanel.OpenCentered();
    }

    public override void Closed()
    {
        CharacterPanel.Close();
    }
}
