using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Shared._CD.CartridgeLoader.Cartridges;
using Content.Shared.Database;
using Content.Shared.IdentityManagement;
using Robust.Server.Player;
using Robust.Shared.Timing;

namespace Content.Server._CD.VoteLink;

/// <summary>
///     Handles the station voting system logic
/// </summary>
public sealed class VoteLinkSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationVoteDatabaseComponent, ComponentInit>(OnDatabaseInit);
    }

    public void HandleMessage(EntityUid station, EntityUid sender, VoteLinkUiMessageEvent msg)
    {
        if (!TryComp<StationVoteDatabaseComponent>(station, out var comp))
            return;

        switch (msg.Type)
        {
            case VoteLinkUiMessageType.Vote:
                TryCastVote(station, sender, msg.Option);
                break;

            case VoteLinkUiMessageType.Create:
                if (msg.VoteData == null)
                    return;

                // Get the options texts from the VoteData
                var options = msg.VoteData.Options
                    .OrderBy(o => o.Option)
                    .Select(o => o.Text)
                    .ToList();

                // Get sender's identity for announcement
                var author = Loc.GetString("comms-console-announcement-unknown-sender"); // recycling
                var tryGetIdentityShortInfoEvent =
                    new TryGetIdentityShortInfoEvent(Transform(sender).MapUid ?? sender, sender);
                RaiseLocalEvent(tryGetIdentityShortInfoEvent);
                if (!string.IsNullOrEmpty(tryGetIdentityShortInfoEvent.Title))
                    author = tryGetIdentityShortInfoEvent.Title;

                // Create and dispatch announcement
                var announcement = Loc.GetString(comp.Message,
                    ("author", author),
                    ("question", msg.VoteData.Question));

                _chat.DispatchGlobalAnnouncement(
                    announcement,
                    Loc.GetString(comp.Title),
                    announcementSound: comp.AnnouncementSound);

                // Log it here so we can get the sender
                _adminLogger.Add(LogType.Action,
                    LogImpact.Medium,
                    $"{ToPrettyString(sender)} Started a new vote: '{msg.VoteData.Question}' with options: {string.Join(", ", options)}");

                // Try to start the vote
                TryStartVote(
                    station,
                    msg.VoteData.Question,
                    options,
                    msg.VoteData.Duration);
                break;
        }
    }

    private void OnDatabaseInit(Entity<StationVoteDatabaseComponent> ent, ref ComponentInit args)
    {
        // Initialize if needed
        UpdateVotes(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StationVoteDatabaseComponent>();
        var curTime = _timing.CurTime;

        while (query.MoveNext(out var uid, out var database))
        {
            if (database.ActiveVote == null || !database.ActiveVote.HasEnded(curTime))
                continue;

            // Move to history
            database.VoteHistory.Add(database.ActiveVote);
            if (database.VoteHistory.Count > database.MaxHistorySize)
                database.VoteHistory.RemoveAt(0);

            database.ActiveVote = null;

            // Set the cooldown
            database.Cooldown = curTime + database.CooldownDuration;

            // Notify that vote has ended
            var ev = new VoteEndedEvent(uid);
            RaiseLocalEvent(ev);
        }
    }

    /// <summary>
    ///     Starts a new vote on the station
    /// </summary>
    public bool TryStartVote(EntityUid station, string question, List<string> options, TimeSpan? duration = null)
    {
        if (!TryComp<StationVoteDatabaseComponent>(station, out var database))
            return false;

        if (database.ActiveVote != null)
            return false;

        // Create new vote with initialized dictionaries for all options including Abstain
        var vote = new VoteData
        {
            Question = question,
            StartTime = _timing.CurTime,
            Options = options.Select((text, i) => new VoteOptionData(
                    text,
                    (VoteOption)i))
                .ToList(),
        };

        if (duration.HasValue)
            vote.Duration = duration.Value;

        // Initialize VotesPerOption with empty sets for all options
        foreach (var option in vote.Options)
        {
            vote.VotesPerOption[option.Option] = new HashSet<NetEntity>();
        }

        // Initialize Abstain option set
        vote.VotesPerOption[VoteOption.Abstain] = new HashSet<NetEntity>();

        // Get all station members and add them to abstain by default
        foreach (var player in _player.Sessions)
        {
            if (player.AttachedEntity is not { } playerEntity)
                continue;

            var playerStation = _station.GetOwningStation(playerEntity);
            if (playerStation != station)
                continue;

            var netEntity = GetNetEntity(playerEntity);
            vote.VotesPerOption[VoteOption.Abstain].Add(netEntity);
        }

        database.ActiveVote = vote;

        var ev = new VoteStartedEvent(station);
        RaiseLocalEvent(ev);

        return true;
    }

    /// <summary>
    ///     Attempts to cast a vote for the given entity
    /// </summary>
    public bool TryCastVote(EntityUid station, EntityUid voter, VoteOption option)
    {
        if (!TryComp<StationVoteDatabaseComponent>(station, out var database) ||
            database.ActiveVote == null)
            return false;

        var netVoter = GetNetEntity(voter);

        // Remove previous vote if exists
        if (database.ActiveVote.HasVoted(netVoter))
        {
            foreach (var voterList in database.ActiveVote.VotesPerOption.Values)
            {
                voterList.Remove(netVoter);
            }
        }

        // Cast new vote - we can now directly access the votes set since it's guaranteed to exist
        if (database.ActiveVote.VotesPerOption.TryGetValue(option, out var voterSet))
            voterSet.Add(netVoter);

        // Notify that vote counts have changed
        var ev = new VoteUpdatedEvent(station);
        RaiseLocalEvent(ev);

        return true;
    }

    private void UpdateVotes(Entity<StationVoteDatabaseComponent> ent)
    {
        var ev = new VoteUpdatedEvent(ent);
        RaiseLocalEvent(ev);
    }
}
