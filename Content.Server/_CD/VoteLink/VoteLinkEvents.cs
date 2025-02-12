namespace Content.Server._CD.VoteLink;

public sealed class VoteStartedEvent(EntityUid station) : EntityEventArgs
{
    public EntityUid Station = station;
}

public sealed class VoteEndedEvent(EntityUid station) : EntityEventArgs
{
    public EntityUid Station = station;
}

public sealed class VoteUpdatedEvent(EntityUid station) : EntityEventArgs
{
    public EntityUid Station = station;
}
