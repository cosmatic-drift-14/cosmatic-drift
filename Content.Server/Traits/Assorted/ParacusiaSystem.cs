using Content.Shared.Traits.Assorted;
using Robust.Shared.Audio;
using Robust.Shared.Timing;

namespace Content.Server.Traits.Assorted;

public sealed class ParacusiaSystem : SharedParacusiaSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public void SetSounds(EntityUid uid, SoundSpecifier sounds, ParacusiaComponent? component = null)
    {
        if (!Resolve(uid, ref component))
        {
            return;
        }
        component.Sounds = sounds;
        Dirty(uid, component);
    }

    public void SetTime(EntityUid uid, float minTime, float maxTime, ParacusiaComponent? component = null)
    {
        if (!Resolve(uid, ref component))
        {
            return;
        }
        component.MinTimeBetweenIncidents = minTime;
        component.MaxTimeBetweenIncidents = maxTime;
        Dirty(uid, component);
    }

    public void SetDistance(EntityUid uid, float maxSoundDistance, ParacusiaComponent? component = null)
    {
        if (!Resolve(uid, ref component))
        {
            return;
        }
        component.MaxSoundDistance = maxSoundDistance;
        Dirty(uid, component);
    }

    public void SetIncidentDelay(EntityUid uid, TimeSpan delayTime, ParacusiaComponent? component = null)
    {
        if (!Resolve(uid, ref component))
        {
            return;
        }

        component.IncidentsDelayedUntil = _timing.CurTime + delayTime;
        Dirty(uid, component);
    }
}
