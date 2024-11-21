using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared._CD.CartridgeLoader.Cartridges;

[Serializable] [NetSerializable]
public sealed class VoteLinkUiState(
    VoteData? activeVote,
    List<VoteData> history,
    TimeSpan? cooldown = null,
    bool hasAccess = false)
    : BoundUserInterfaceState
{
    public readonly VoteData? ActiveVote = activeVote;
    public readonly List<VoteData> VoteHistory = history;
    public readonly TimeSpan? Cooldown = cooldown;
    public readonly bool HasAccess = hasAccess;
}

[Serializable] [NetSerializable]
public struct VoteOptionData(string text, VoteOption option, int votes = 0)
{
    public readonly string Text = text;
    public readonly VoteOption Option = option;
    public readonly int Votes = votes;
}

[DataDefinition] [Serializable] [NetSerializable]
public sealed partial class VoteData
{
    /// <summary>
    ///     The question asked in the poll.
    /// </summary>
    [DataField(required: true)]
    public string Question = default!;

    /// <summary>
    ///     Possible options.
    /// </summary>
    [DataField(required: true)]
    public List<VoteOptionData> Options = new();

    /// <summary>
    ///     The <see cref="IGameTiming.CurTime" /> timespan of when the vote was started.
    /// </summary>
    [DataField]
    public TimeSpan StartTime;

    /// <summary>
    ///     The duration of the vote.
    /// </summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromMinutes(2);

    /// <summary>
    ///     Dictionary of votes for each of VoteOptions.
    /// </summary>
    [DataField]
    public Dictionary<VoteOption, HashSet<NetEntity>> VotesPerOption = new();

    // Helper methods
    public bool HasEnded(TimeSpan currentTime)
    {
        return currentTime >= StartTime + Duration;
    }

    public TimeSpan TimeRemaining(TimeSpan currentTime)
    {
        var remaining = StartTime + Duration - currentTime;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    public int GetVoteCount(VoteOption option)
    {
        return VotesPerOption.TryGetValue(option, out var votes) ? votes.Count : 0;
    }

    public bool HasVoted(NetEntity voter)
    {
        foreach (var votes in VotesPerOption.Values)
        {
            if (votes.Contains(voter))
                return true;
        }

        return false;
    }

    public VoteOption? GetCurrentVote(NetEntity voter)
    {
        foreach (var (option, votes) in VotesPerOption)
        {
            if (votes.Contains(voter))
                return option;
        }

        return null;
    }
}
