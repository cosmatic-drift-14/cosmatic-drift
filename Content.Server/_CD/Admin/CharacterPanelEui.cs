using Content.Server.Administration;
using Content.Server.EUI;
using Content.Shared._CD.Admin;
using Content.Shared.Eui;

namespace Content.Server._CD.Admin;

public sealed class CharacterPanelEui : BaseEui
{
    private readonly LocatedPlayerData _targetPlayer;

    public CharacterPanelEui(LocatedPlayerData player)
    {
        IoCManager.InjectDependencies(this);
        _targetPlayer = player;
    }

    public override EuiStateBase GetNewState()
    {
        return new CharacterPanelEuiState(_targetPlayer.Username);
    }
}
