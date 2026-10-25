using Content.Server.Chat.Systems;
using Content.Shared._CD.Speech.Components;
using Content.Shared.Chat;
using Content.Shared.Speech;

// ReSharper disable once CheckNamespace
namespace Content.Server.Speech.EntitySystems;

public sealed partial class ListeningSystem
{
    private void OnEmote(ref CDEntityEmotedEvent ev)
    {
        PingEmoteListeners(ev.Source, ev.Action);
    }

    /// <summary>
    /// A copy of <see cref="PingListeners"/> instead of integrating into the original method for the
    /// sake of maintaining.
    /// </summary>
    // Above might be unwise?
    public void PingEmoteListeners(EntityUid source, string action)
    {
        var sourceXform = Transform(source);
        var sourcePos = _xforms.GetWorldPosition(sourceXform);

        var attemptEv = new ListenAttemptEvent(source);
        var ev = new ListenEvent(action, source, ChatChannel.Emotes);
        var query = EntityQueryEnumerator<CDActiveEmoteListenerComponent, TransformComponent>();

        while (query.MoveNext(out var listenerUid, out var listener, out var xform))
        {
            if (xform.MapID != sourceXform.MapID)
                continue;

            // range checks
            // TODO proper speech occlusion
            var distance = (sourcePos - _xforms.GetWorldPosition(xform)).LengthSquared();
            if (distance > listener.Range * listener.Range)
                continue;

            RaiseLocalEvent(listenerUid, attemptEv);
            if (attemptEv.Cancelled)
            {
                attemptEv.Uncancel();
                continue;
            }

            RaiseLocalEvent(listenerUid, ev);
        }
    }
}
