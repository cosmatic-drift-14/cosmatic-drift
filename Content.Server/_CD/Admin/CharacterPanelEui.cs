using Content.Server.Administration;
using Content.Server.DetailExaminable;
using Content.Server.EUI;
using Content.Shared._CD.Admin;
using Content.Shared.Eui;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Admin;

public sealed class CharacterPanelEui : BaseEui
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;


    private readonly LocatedPlayerData _targetPlayer;
    private string? _description;

    private HumanoidCharacterProfile _pref;
    private readonly List<AntagPrototype> _visibleAntagPrototypes;

    public CharacterPanelEui(LocatedPlayerData player, HumanoidCharacterProfile pref)
    {
        IoCManager.InjectDependencies(this);
        _targetPlayer = player;
        _pref = pref;

        var visibleAntagPrototypes = new List<AntagPrototype>();
        foreach (var antagPrototype in _proto.EnumeratePrototypes<AntagPrototype>())
        {
            if(antagPrototype.VisiblePreference)
                visibleAntagPrototypes.Add(antagPrototype);
        }

        _visibleAntagPrototypes = visibleAntagPrototypes;
    }

    public override EuiStateBase GetNewState()
    {
        return new CharacterPanelEuiState(
            _targetPlayer.UserId,
            _targetPlayer.Username,
            _description,
            _pref,
            _visibleAntagPrototypes
            );
    }

    public async void SetPlayerState()
    {
        if (_player.TryGetSessionById(_targetPlayer.UserId, out var session))
        {
            if (_entity.TryGetComponent<DetailExaminableComponent>(session.AttachedEntity, out var examinable))
                _description = examinable.Content;
        }

        StateDirty();
    }
}
