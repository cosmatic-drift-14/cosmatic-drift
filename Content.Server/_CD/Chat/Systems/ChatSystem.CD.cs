// ReSharper disable once CheckNamespace
namespace Content.Server.Chat.Systems;

[ByRefEvent]
public record struct CDEntityEmotedEvent(EntityUid Source, string Action);
