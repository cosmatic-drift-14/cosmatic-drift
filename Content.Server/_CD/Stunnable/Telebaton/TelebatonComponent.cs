using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Server._CD.Stunnable.Telebaton;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
[Access(typeof(SharedTelebatonSystem))]
public sealed partial class TelebatonComponent : Component
{
    [DataField("activated"), ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool Activated = false;

    [DataField("sparksSound")]
    public SoundSpecifier SparksSound = new SoundCollectionSpecifier("sparks");
}