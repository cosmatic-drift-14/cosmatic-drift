using Content.Client.Lobby;
using Content.Shared._CD;
using Content.Shared.Botany;
using Robust.Client.GameStates;
using Robust.Client.State;

namespace Content.Client._CD;

public sealed partial class ExtendableClothingSystem : SharedExtendableClothingSystem
{
    [Dependency] private readonly IStateManager _stateManager = default!;

    protected override bool IsPlayerInLobby()
    {
        return _stateManager.CurrentState is LobbyState;
    }
}
