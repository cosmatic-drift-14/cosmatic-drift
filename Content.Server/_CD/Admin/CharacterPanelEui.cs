using Content.Server.Administration;
using Content.Server.DetailExaminable;
using Content.Server.EUI;
using Content.Shared._CD.Admin;
using Content.Shared.Eui;

namespace Content.Server._CD.Admin;

public sealed class CharacterPanelEui : BaseEui
{
    private readonly IEntityManager _entity = default!;

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
        if (!_entity.TryGetComponent<DetailExaminableComponent>(Player.AttachedEntity, out DetailExaminableComponent? examinable))
            _description = null;
        else
            _description = examinable.Content;

        StateDirty();
    }
}
