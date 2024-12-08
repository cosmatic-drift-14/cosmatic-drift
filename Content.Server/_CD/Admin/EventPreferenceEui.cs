using Content.Server.Administration;
using Content.Server.EUI;
using Content.Shared.Preferences;

namespace Content.Server._CD.Admin;

public sealed class EventPreferenceEui : BaseEui
{
    private readonly HumanoidCharacterProfile _playerPref;
    private readonly LocatedPlayerData _targetPlayer;

    public EventPreferenceEui(LocatedPlayerData player, HumanoidCharacterProfile pref)
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
