using Content.Client.Administration.Managers;
using Content.Client.Eui;
using Content.Shared._CD.Admin;
using Content.Shared.Eui;

namespace Content.Client._CD.Admin.UI.CharacterPanel;

public sealed class CharacterPanelEui : BaseEui
{
    [Dependency] private readonly IClientAdminManager _admin = default!;

    private CharacterPanel CharacterPanel { get; }

    public CharacterPanelEui()
    {
        CharacterPanel = new CharacterPanel(_admin);
    }

    public override void Opened()
    {
        CharacterPanel.OpenCentered();
    }

    public override void Closed()
    {
        CharacterPanel.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not CharacterPanelEuiState s)
            return;

        CharacterPanel.TargetPlayer = s.Player;
        CharacterPanel.SetPlayer(s.Player);
        CharacterPanel.SetCharacterDescription(s.CharacterDescription);
    }
}
