using Robust.Shared.GameStates;

namespace Content.Shared._CD.Species;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CustomSpeciesNameComponent : Component
{
    [AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public string NewSpeciesName = string.Empty;
}
