using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Shared._CD;
using JetBrains.Annotations;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Configuration;
using Robust.Shared.Utility;

namespace Content.Client._CD.WelcomeMessage;

[UsedImplicitly]
public sealed class WelcomeWindowUIController : UIController, IOnStateEntered<GameplayState>
{
    [Dependency] private readonly GameplayStateLoadController _gameplayStateLoad = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    private const string ContentsPath = "/ServerInfo/InGameWelcome.xml";

    private WelcomeWindow? _window;
    private int _currentHash;

    public override void Initialize()
    {
        base.Initialize();

        _gameplayStateLoad.OnScreenLoad += () =>
        {
            _window = UIManager.CreateWindow<WelcomeWindow>();
            _currentHash = _window.LoadContents(new ResPath(ContentsPath));

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
        var lastSeen = _config.GetCVar(CDCvars.WelcomePopupLastSeen);
        if (_currentHash == lastSeen)
        {
            return;
        }

        _window?.Open();
    }

    private void OnDoNotShow()
    {
        _window?.Close();
        _config.SetCVar(CDCvars.WelcomePopupLastSeen, _currentHash);
        _config.SaveToFile();
    }
}
