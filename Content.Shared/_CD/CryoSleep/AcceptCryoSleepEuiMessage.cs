using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.CryoSleep;

[Serializable, NetSerializable]
public enum AcceptCryoUiButton
{
    Deny,
    Accept,
}

[Serializable, NetSerializable]
public sealed class AcceptCryoChoiceMessage(AcceptCryoUiButton button) : EuiMessageBase
{
    public readonly AcceptCryoUiButton Button = button;
}
