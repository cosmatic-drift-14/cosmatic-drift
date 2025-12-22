using Content.Shared._CD.Silicons.StationAi;
using Robust.Client.GameObjects;

namespace Content.Client._CD.Silicons.StationAi;

public sealed class StationAiShellUserSystem : SharedStationAiShellUserSystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationAiShellUserComponent, AfterAutoHandleStateEvent>(OnShellUserState);
    }

    private void OnShellUserState(Entity<StationAiShellUserComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (UserInterface.TryGetOpenUi<AiShellSelectionBoundUserInterface>(ent.Owner, ShellUiKey.Key, out var bui))
        {
            bui.Refresh(ent);
        }
    }
}
