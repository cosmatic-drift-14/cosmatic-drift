using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Communications;
using Content.Server.RoundEnd;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared._CD.CCVars;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Dataset;
using Content.Shared.GameTicking;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CD.RoundEnd;

public sealed class RoundEndVoteSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IVoteManager _voteManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

    private static readonly ProtoId<LocalizedDatasetPrototype> VoteUserDataset = "ShuttleVoteUserDataset";

    [ViewVariables]
    public TimeSpan RoundEndTime { get; private set; }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

        // Set the round restart time just in case the vote does not work for whatever reason.
        _cfg.OnValueChanged(CCVars.RoundRestartTime, (value) => RoundEndTime = TimeSpan.FromSeconds(value), true);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        // Reset the round end time to the CVar in case the round is ended not though the shuttle during the next round.
        RoundEndTime = TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.RoundRestartTime));
    }

    private string GetVoteUserName()
    {
        if (!_prototypeManager.TryIndex(VoteUserDataset, out var voteUsers))
        {
            Log.Error("Could not find vote user dataset");
            return "Server"; // This should never happen
        }

        return Loc.GetString(_random.Pick(voteUsers.Values));
    }

    /// <summary>
    /// Run an OOC shuttle vote
    /// </summary>
    public void RunRestartVote()
    {
        var options = new VoteOptions
        {
            InitiatorText = GetVoteUserName(),
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

                _roundEndSystem.RequestRoundEnd(null, false, "round-end-system-shuttle-auto-called-announcement");
            }
            else
            {
                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Restart vote failed: {votesYes}/{votesNo}");
            }
        };

    }

    /// <summary>
    /// Vote OOCly how long CentCom should be.
    /// </summary>
    public void RunRoundEndTimeVote()
    {
        var normalRoundRestartTime = _cfg.GetCVar(CCVars.RoundRestartTime);
        var roundRestartDelta = _cfg.GetCVar(CDCCVars.RoundRestartTimeVoteDelta);

        float[] voteValues =
        [
            (normalRoundRestartTime - roundRestartDelta),
            (normalRoundRestartTime),
            (normalRoundRestartTime + roundRestartDelta),
        ];

        var options = new VoteOptions
        {
            InitiatorText = GetVoteUserName(),
            Title = Loc.GetString("end-time-vote-title"),
            Duration = TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteTimerRestart)),
            DisplayVotes = false,
        };

        for (var i = 0; i < voteValues.Length; i++)
        {
            voteValues[i] /= 60.0f; // Change from seconds to minutes

            // We can cast to integer for this because we don't intend to offer non-integer vote times.
            options.Options.Add((((int)voteValues[i]).ToString(), i));
        }

        var vote = _voteManager.CreateVote(options);

        vote.OnFinished += (_, _) =>
        {
            var votes = new int[voteValues.Length];
            var acc = 0.0f;
            for (var i = 0; i < voteValues.Length; i++)
            {
                votes[i] = vote.VotesPerOption[i];
                acc += votes[i] * voteValues[i];
            }

            var numVotes = votes.Sum();
            if (numVotes <= 0)
                return;

            var endTimeMinutes = float.Round(acc / numVotes);
            RoundEndTime = TimeSpan.FromMinutes(endTimeMinutes);

            _chatManager.DispatchServerAnnouncement(Loc.GetString("end-time-vote-announcement", ("minutes", endTimeMinutes)));
        };
    }
}
