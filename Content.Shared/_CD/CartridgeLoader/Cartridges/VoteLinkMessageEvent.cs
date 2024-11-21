using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.CartridgeLoader.Cartridges;

[Serializable] [NetSerializable]
public sealed class VoteLinkUiMessageEvent(VoteLinkUiMessageType type, VoteOption option, VoteData? voteData = null)
    : CartridgeMessageEvent
{
    public readonly VoteLinkUiMessageType Type = type;
    public readonly VoteOption Option = option;
    public readonly VoteData? VoteData = voteData;
}

[Serializable] [NetSerializable]
public enum VoteLinkUiMessageType : byte
{
    Vote,
    Create,
    RequestHistory,
}

[Serializable] [NetSerializable]
public enum VoteOption : byte
{
    Option1,
    Option2,
    Option3,
    Option4,
    Abstain,
}
