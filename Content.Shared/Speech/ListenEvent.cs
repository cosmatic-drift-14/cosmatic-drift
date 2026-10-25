using Content.Shared.Chat;

namespace Content.Shared.Speech;

public sealed class ListenEvent : EntityEventArgs
{
    public readonly string Message;
    public readonly EntityUid Source;
    public readonly ChatChannel ChatType; // CD

    public ListenEvent(string message, EntityUid source, ChatChannel chatType = ChatChannel.Local) // CD chatType change
    {
        Message = message;
        Source = source;
        ChatType = chatType; // CD
    }
}

public sealed class ListenAttemptEvent : CancellableEntityEventArgs
{
    public readonly EntityUid Source;

    public ListenAttemptEvent(EntityUid source)
    {
        Source = source;
    }
}
