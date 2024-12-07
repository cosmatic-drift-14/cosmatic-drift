using Content.Server.Administration;
using Content.Server.DetailExaminable;
using Content.Server.EUI;
using Content.Shared._CD.Admin;
using Content.Shared.Eui;
using Robust.Server.Player;

namespace Content.Server._CD.Admin;

public sealed class CharacterPanelEui : BaseEui
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly LocatedPlayerData _targetPlayer;
    private string? _description;

    public CharacterPanelEui(LocatedPlayerData player)
    {
        IoCManager.InjectDependencies(this);
        _targetPlayer = player;
    }

    public override EuiStateBase GetNewState()
    {
        return new CharacterPanelEuiState(
            _targetPlayer.UserId,
            _targetPlayer.Username,
            _description
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
