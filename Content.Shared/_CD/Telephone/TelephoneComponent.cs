using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._CD.Telephone;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedTelephoneSystem))]
public sealed partial class TelephoneComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? RotaryPhone;

    [DataField, AutoNetworkedField]
    public SoundSpecifier SpeakSound = new SoundCollectionSpecifier("CDPhoneSpeak", AudioParams.Default.WithVolume(-3));
}
