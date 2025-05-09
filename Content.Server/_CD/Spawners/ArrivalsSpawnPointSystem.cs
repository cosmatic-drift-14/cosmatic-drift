using System.Linq;
using Content.Server.GameTicking;
using Content.Shared.GameTicking;
using Robust.Shared.Map;

using Robust.Shared.Random;

namespace Content.Server._CD.Spawners;

public sealed class ArrivalsSpawnPointSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        // Ensure they have a job, so that we won't end up making mobs spawn on arrivals.
        if (args.JobId == null)
            return;

        var generalSpawns = new List<Entity<ArrivalsSpawnPointComponent>>();
        var jobSpawns = new List<Entity<ArrivalsSpawnPointComponent>>();

        var query = EntityQueryEnumerator<ArrivalsSpawnPointComponent>();
        while (query.MoveNext(out var spawnUid, out var spawnPoint))
        {
            if (spawnPoint.JobIds.Count == 0)
            {
                generalSpawns.Add((spawnUid, spawnPoint));
                continue;
            }

            jobSpawns.Add((spawnUid, spawnPoint));
        }

        _random.Shuffle(jobSpawns);

        foreach (var (spawnUid, spawnPoint) in jobSpawns)
        {
            foreach (var ignoredJob in spawnPoint.IgnoredJobs)
            {
                if (args.JobId == ignoredJob)
                    return;
            }

            foreach(var jobId in spawnPoint.JobIds!)
            {
                if (jobId == args.JobId)
                {
                    _transform.SetCoordinates(args.Mob, Transform(spawnUid).Coordinates);
                    return;
                }
            }
        }

        if(generalSpawns.Count == 0)
            return;

        _random.Shuffle(generalSpawns);
        var spawn = generalSpawns.First();

        foreach (var ignoredJob in spawn.Comp.IgnoredJobs)
        {
            if (args.JobId == ignoredJob)
                return;
        }

        var xform = Transform(spawn);
        _transform.SetCoordinates(args.Mob, xform.Coordinates);

        // Unpause the map if it's paused. We don't want people spawning on paused maps.
        if(_mapManager.IsMapPaused(xform.MapID))
            _mapManager.SetMapPaused(xform.MapID, false);

        return;
    }
}
