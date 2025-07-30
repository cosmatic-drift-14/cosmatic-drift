using System.Runtime.CompilerServices;
using Content.Shared._CD.Silicons.StationAi;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._CD.Silicons.StationAi;
[UsedImplicitly]
public sealed class AiShellSelectionBoundUserInterface : BoundUserInterface
{
    private AiShellSelectionMenu? _window;
    public AiShellSelectionBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) {}

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<AiShellSelectionMenu>();

        _window.JumpToShell += OnJumpToShell;
        _window.EnterShell += OnEnterShell;
        _window.SelectShell += OnSelectShell;

        if (EntMan.TryGetComponent(Owner, out StationAiShellUserComponent? component))
        {
            Refresh((Owner, component));
        }
    }

    private void OnSelectShell(NetEntity? netEntity)
    {
        SendMessage(new SelectShellMessage(netEntity));
    }

    private void OnJumpToShell(NetEntity? netEntity)
    {
        SendMessage(new JumpToShellMessage(netEntity));
    }

    private void OnEnterShell(NetEntity? netEntity)
    {
        SendPredictedMessage(new EnterShellMessage(netEntity));
    }

    public void Refresh(Entity<StationAiShellUserComponent> ent)
    {
        if(_window == null)
            return;

        _window.Populate(ent);
    }
}
