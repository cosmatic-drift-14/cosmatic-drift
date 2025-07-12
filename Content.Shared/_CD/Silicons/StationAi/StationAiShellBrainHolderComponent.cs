using Robust.Shared.GameStates;

namespace Content.Shared._CD.Silicons.StationAi;

[AutoGenerateComponentState]
[RegisterComponent, NetworkedComponent]

public sealed partial class StationAiShellBrainHolderComponent : Component
{
    [AutoNetworkedField]
    public EntityUid Brain { get; set; }
}
