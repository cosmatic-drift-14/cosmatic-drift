using Robust.Shared.GameStates;

namespace Content.Shared._CD.Telephone;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedTelephoneSystem))]
public sealed partial class RotaryPhoneReceivingComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Other;
}
