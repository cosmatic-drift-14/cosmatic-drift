using Content.Shared._CD.JobSlotsConsole;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Client._CD.JobSlotsConsole;

public sealed class JobSlotsConsoleBoundUserInterface : BoundUserInterface
{

    private JobSlotsConsoleMenu? _menu;

    public JobSlotsConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = new JobSlotsConsoleMenu();
        _menu.OpenCentered();
        _menu.OnClose += Close;
        _menu.OnAdjustPressed += AdjustSlot;
    }

    private void AdjustSlot(ProtoId<JobPrototype> jobId, JobSlotAdjustment adjustment)
    {
        SendMessage(new JobSlotsConsoleAdjustMessage(jobId, adjustment));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not JobSlotsConsoleState cast)
            return;

        _menu?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Close();
        _menu = null;
    }
}
