using System.Linq;
using Content.Server.Administration;
using Content.Server.EUI;
using Content.Server.Preferences.Managers;
using Content.Shared.Administration;
using Content.Shared.Preferences;
using Robust.Server.Player;
using Robust.Shared.Console;

namespace Content.Server._CD.Admin.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class EventPreferencesCommand : LocalizedCommands
{
    [Dependency] private readonly IPlayerLocator _locator = default!;
    [Dependency] private readonly IServerPreferencesManager _pref = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    public override string Command => "eventpreferences";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } admin)
        {
            shell.WriteError(Loc.GetString("cmd-playerpanel-server"));
            return;
        }

        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("cmd-playerpanel-invalid-arguments"));
            return;
        }

        var queriedPlayer = await _locator.LookupIdByNameAsync(args[0]);

        if (queriedPlayer == null)
        {
            shell.WriteError(Loc.GetString("cmd-playerpanel-invalid-player"));
            return;
        }

        var pref = (HumanoidCharacterProfile)_pref.GetPreferences(queriedPlayer.UserId).SelectedCharacter;

        var ui = new EventPreferencesPanelEui(queriedPlayer, pref);
        _eui.OpenEui(ui, admin);
        ui.StateDirty();
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            var options = _players.Sessions.OrderBy(c => c.Name).Select(c => c.Name).ToArray();

            return CompletionResult.FromHintOptions(options, LocalizationManager.GetString("cmd-playerpanel-completion"));
        }

        return CompletionResult.Empty;
    }
}
