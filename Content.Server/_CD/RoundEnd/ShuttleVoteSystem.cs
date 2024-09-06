using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.RoundEnd;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Dataset;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._CD.RoundEnd;

public sealed class ShuttleVoteSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IVoteManager _voteManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;

    private static readonly ProtoId<LocalizedDatasetPrototype> VoteUserDataset = "ShuttleVoteUserDataset";

    /// <summary>
    /// Run an OOC shuttle vote
    /// </summary>
    public void RunRestartVote()
    {
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
        };

        var vote = _voteManager.CreateVote(options);

        vote.OnFinished += (_, _) =>
        {
            var votesYes = vote.VotesPerOption["yes"];
            var votesNo = vote.VotesPerOption["no"];
            var total = votesYes + votesNo;

            if (total > 0 && votesYes >= votesNo)
            {
                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Round end shuttle vote succeded: {votesYes}/{votesNo}");
                // TODO: Add .loc files n make an unrecallable shuttle
                _chatManager.DispatchServerAnnouncement(Loc.GetString("Vote succeeded, round end shuttle enroute"));
                // This is kinda cursed but whatever, stops a recall
                _cfg.SetCVar(CCVars.EmergencyRecallTurningPoint, 0f);

                _roundEndSystem.RequestRoundEnd(null, false, "round-end-system-shuttle-auto-called-announcement");
            }
            else
            {
                _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"Restart vote failed: {votesYes}/{votesNo}");
            }
        };

    }
}
