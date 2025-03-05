using Content.Shared._CD.CryoSleep;
using Robust.Client.Player;

namespace Content.Client._CD.CryoSleep;

public sealed class LostAndFoundBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private LostAndFoundWindow? _window;

    public LostAndFoundBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _window = new LostAndFoundWindow();
        _window.OnRetrieveItem += OnRetrieveItem;
    }

    private void OnRetrieveItem(string playerName, string slotName)
    {
        if ( _playerManager.LocalEntity is not { } user)
            return;

        var netEntity = _entManager.GetNetEntity(user);
        SendMessage(new LostAndFoundRetrieveItemMessage(playerName, slotName, netEntity));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is LostAndFoundBuiState msg)
            _window?.Populate(msg.StoredPlayers);
    }

    protected override void Open()
    {
        base.Open();
        _window?.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _window?.Close();
        _window = null;
    }
}
