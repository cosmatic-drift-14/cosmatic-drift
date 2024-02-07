using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Gameplay;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;

namespace Content.Client._CD.WelcomeMessage;

[UsedImplicitly]
public sealed class WelcomeWindowUIController : UIController, IOnStateEntered<GameplayState>
{
    [Dependency] private readonly GameplayStateLoadController _gameplayStateLoad = default!;
    [Dependency] private readonly IResourceManager _resource = default!;

    // Please try to remember to increment this when the file below is changed. Sadly, there is no way
    // to leave a comment in the scuffed XML-robust markup hybrid ss14 uses.
    private const int TextRevision = 1;
    private const string ContentsPath = "/ServerInfo/InGameWelcome.xml";

    private const string LastSeenRevisionPath = "/cosmatic_welcome_last_seen";

    private WelcomeWindow? _window;

    public override void Initialize()
    {
        base.Initialize();

        _gameplayStateLoad.OnScreenLoad += () =>
        {
            _window = UIManager.CreateWindow<WelcomeWindow>();
            _window.LoadContents(new ResPath(ContentsPath));

            _window.NoShowButton.OnPressed += _ => OnDoNotShow();
        };

        _gameplayStateLoad.OnScreenUnload += () =>
        {
            if (_window != null)
            {
                _window.Dispose();
                _window = null;
            }
        };
    }

    public void OnStateEntered(GameplayState state)
    {
        if (_resource.UserData.TryReadAllText(new ResPath(LastSeenRevisionPath), out var txt))
        {
            if (int.TryParse(txt, out var val) && val >= TextRevision)
            {
                return;
            }
        }
        _window?.Open();
    }

    private void OnDoNotShow()
    {
        _window?.Close();
        _resource.UserData.WriteAllText(new ResPath(LastSeenRevisionPath), TextRevision.ToString());
    }
}
