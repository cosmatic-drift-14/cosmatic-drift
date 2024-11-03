using Robust.Shared.GameStates;

namespace Content.Shared._CD.Telephone;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedTelephoneSystem))]
public sealed partial class PickedUpPhoneComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Range = 7;
}
