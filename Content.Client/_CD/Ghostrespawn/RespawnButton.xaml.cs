using Content.Client.Ghost;
using Content.Shared._CD.CCVars;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Client._CD.Ghostrespawn;

public sealed class RespawnButton : Button
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;

    private GhostSystem? _ghost;

    private readonly float _minTimeToRespawn;
    private GhostRespawnRulesWindow _window = new();

    public RespawnButton()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        _minTimeToRespawn = _cfg.GetCVar(CDCCVars.RespawnTime);

        OnPressed += _ => _window.OpenCentered();
    }

    protected override void EnteredTree()
    {
        base.EnteredTree();
        // Shitty hack to access entity systems in UI. I know this is bad code but the other option is poluting wizden code.
        _ghost = _entMan.System<GhostSystem>();
    }

    protected override void ExitedTree()
    {
        base.ExitedTree();

        _ghost = null;
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        base.FrameUpdate(args);

        var timeOfDeath = _ghost?.Player?.TimeOfDeath;
        if (timeOfDeath is null)
        {
            Text = Loc.GetString("ghost-gui-respawn-button-denied", ("time", "disabled"));
            Disabled = true;
            return;
        }

        var delta = _minTimeToRespawn - _gameTiming.CurTime.Subtract(timeOfDeath.Value).TotalSeconds;
        if (delta <= 0)
        {
            Text = Loc.GetString("ghost-gui-respawn-button-allowed");
            Disabled = false;
        }
        else
        {
            Text = Loc.GetString("ghost-gui-respawn-button-denied", ("time", $"{delta:f1}"));
            Disabled = true;
        }
    }
}
