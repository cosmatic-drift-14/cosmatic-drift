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

        if (EntMan.TryGetComponent(Owner, out StationAiShellUserComponent? component))
        {
            Refresh((Owner, component));
        }
    }

    private void OnJumpToShell(NetEntity? uid)
    {
        SendPredictedMessage(new JumpToShellMessage(uid));
    }

    private void OnEnterShell(NetEntity? uid)
    {
        SendPredictedMessage(new EnterShellMessage(uid));
    }

    public void Refresh(Entity<StationAiShellUserComponent> ent)
    {
        if(_window == null)
            return;

        _window.Populate(ent.Comp.ControllableShells);
    }
}
