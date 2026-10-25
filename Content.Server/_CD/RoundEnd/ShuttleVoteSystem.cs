using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Communications;
using Content.Server.RoundEnd;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Dataset;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CD.RoundEnd;

public sealed partial class ShuttleVoteSystem : EntitySystem
{
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IVoteManager _voteManager = default!;
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private RoundEndSystem _roundEndSystem = default!;

    // used as part of the tempoary hack below
    [Dependency] private INetManager _netMan = default!;

    private static readonly ProtoId<LocalizedDatasetPrototype> VoteUserDataset = "ShuttleVoteUserDataset";

    /// <summary>
    /// Run an OOC shuttle vote
    /// </summary>
    public void RunRestartVote()
    {
        // HACK: Remove if https://github.com/space-wizards/RobustToolbox/pull/6603 is merged or if RT devs provide a better idea.
        // The testing ICommonSession (DummySession. crashes when we create a vote. Since this happens on a timer it causes our tests
        // to be flakey. This is disgusting but it works. We check NetManager since it is one of the easiest systems to test if we are in a
        // testing environ.
        //
        // I am well aware this is a crime against humanity.
        // -- aquif Fri May 29 08:56:16 PM CDT 2026
        if (_netMan is not NetManager)
            return;

        if (!_prototypeManager.TryIndex(VoteUserDataset, out var voteUsers))
            return;

        var voteUserLoc = _random.Pick(voteUsers.Values);
        var options = new VoteOptions
        {
            InitiatorText = Loc.GetString(voteUserLoc),
            Title = Loc.GetString("shuttle-vote-title"),
            Options =
            {
                (Loc.GetString("ui-vote-restart-yes"), "yes"),
                (Loc.GetString("ui-vote-restart-no"), "no"),
                (Loc.GetString("ui-vote-restart-abstain"), "abstain"),
            },
            Duration = TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteTimerRestart)),
            DisplayVotes = false,
        };

        var vote = _voteManager.CreateVote(options);

        vote.OnFinished += (_, _) =>
        {
            var votesYes = vote.VotesPerOption["yes"];
            var votesNo = vote.VotesPerOption["no"];
            var total = votesYes + votesNo;

            if (total > 0 && votesYes >= votesNo)
            {
                // TODO: Add .loc files
                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Round end shuttle vote succeded: {votesYes}/{votesNo}");
                _chatManager.DispatchServerAnnouncement(Loc.GetString("Vote succeeded, round end shuttle enroute"));

                // Disable the ability to recall the shuttle on all existing consoles. Players could technically bypass
                // this by building a new one. However, I don't think this is likely enough to warrant accounting for it
                // and would be an admin issue is somebody was abusing it.
                foreach (var console in EntityQuery<CommunicationsConsoleComponent>())
                {
                    console.CanShuttle = false;
                }

                _roundEndSystem.RequestRoundEnd(null, null, false, "round-end-system-shuttle-auto-called-announcement");
            }
            else
            {
                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Restart vote failed: {votesYes}/{votesNo}");
            }
        };

    }
}
