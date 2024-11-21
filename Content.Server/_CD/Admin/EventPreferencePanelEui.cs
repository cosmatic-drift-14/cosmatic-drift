using System.Linq;
using Content.Server.Administration;
using Content.Server.EUI;
using Content.Shared._CD.Admin;
using Content.Shared.Eui;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Admin;

public sealed class EventPreferencePanelEui : BaseEui
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly HumanoidCharacterProfile _playerPref;
    private readonly LocatedPlayerData _targetPlayer;
    private List<AntagPrototype> _visibleAntagPrototypes = new List<AntagPrototype>();

    public EventPreferencePanelEui(LocatedPlayerData player, HumanoidCharacterProfile pref)
    {
        IoCManager.InjectDependencies(this);
        _targetPlayer = player;
        _playerPref = pref;
    }

    public override EuiStateBase GetNewState()
    {
        return new EventPreferencePanelEuiState(
            _targetPlayer.Username,
            _playerPref,
            _visibleAntagPrototypes
            );
    }

    public async void SetState()
    {
        var visibleAntagPrototypes = new List<AntagPrototype>();
        foreach (var antagPrototype in _proto.EnumeratePrototypes<AntagPrototype>())
        {
            if(antagPrototype.VisiblePreference)
                visibleAntagPrototypes.Add(antagPrototype);
        }

        _visibleAntagPrototypes = visibleAntagPrototypes;

        StateDirty();
    }
}
