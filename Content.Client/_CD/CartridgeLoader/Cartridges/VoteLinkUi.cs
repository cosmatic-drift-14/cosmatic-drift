using Content.Client.UserInterface.Fragments;
using Content.Shared._CD.CartridgeLoader.Cartridges;
using Content.Shared.CartridgeLoader;
using Robust.Client.UserInterface;

namespace Content.Client._CD.CartridgeLoader.Cartridges;

public sealed partial class VoteLinkUi : UIFragment
{
    private VoteLinkUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new VoteLinkUiFragment();

        _fragment.OnOptionSelected += option =>
        {
            SendVoteLinkUiMessage(VoteLinkUiMessageType.Vote, option, null, userInterface);
        };

        _fragment.OnCreateVote += voteData =>
        {
            SendVoteLinkUiMessage(VoteLinkUiMessageType.Create, VoteOption.Option1, voteData, userInterface);
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is VoteLinkUiState cast)
            _fragment?.UpdateState(cast);
    }

    private static void SendVoteLinkUiMessage(VoteLinkUiMessageType type,
        VoteOption option,
        VoteData? voteData,
        BoundUserInterface userInterface)
    {
        var voteMessage = new VoteLinkUiMessageEvent(type, option, voteData);
        var message = new CartridgeUiMessage(voteMessage);
        userInterface.SendMessage(message);
    }
}
