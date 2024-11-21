using Content.Server.Administration;
using Content.Server.EUI;
using Content.Shared.Preferences;

namespace Content.Server._CD.Admin;

public sealed class EventPreferencePanelEui : BaseEui
{
    private readonly HumanoidCharacterProfile _playerPref;
    private readonly LocatedPlayerData _targetPlayer;

    public EventPreferencePanelEui(LocatedPlayerData player, HumanoidCharacterProfile pref)
    {
        IoCManager.InjectDependencies(this);
        _targetPlayer = player;
        _playerPref = pref;
    }

    public async void SetState()
    {
        StateDirty();
    }
}
