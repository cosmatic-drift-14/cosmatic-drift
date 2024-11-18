using Content.Server.Administration;
using Content.Server.EUI;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._CD.Admin.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class CharacterPanel : LocalizedCommands
{
    [Dependency] private readonly EuiManager _euis = default!;
    public override string Command => "characterpanel";
    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } admin)
        {
            shell.WriteError(Loc.GetString("cmd-playerpanel-server"));
            return;
        }

        var ui = new CharacterPanelEui();
        _euis.OpenEui(ui, admin);
    }
}
