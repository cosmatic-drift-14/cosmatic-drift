using Content.Server.Station.Systems;
using Content.Shared._CD.JobSlotsConsole;
using Robust.Server.GameObjects;
using System.Linq;
using Content.Server.Station.Components;
using Content.Shared.GameTicking;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.JobSlotsConsole;

public sealed class JobSlotsConsoleSystem : EntitySystem
{
    [Dependency] private readonly StationJobsSystem _jobs = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly StationSystem _station = default!;

    // Keep track of consoles so we can update them
    private readonly Dictionary<EntityUid, HashSet<EntityUid>> _stationConsoles = new();

    // Track the last known job counts for each station to detect changes
    private readonly Dictionary<EntityUid, Dictionary<ProtoId<JobPrototype>, int?>> _lastKnownJobs = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<JobSlotsConsoleComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<JobSlotsConsoleComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<JobSlotsConsoleComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<JobSlotsConsoleComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<JobSlotsConsoleComponent, JobSlotsConsoleAdjustMessage>(OnAdjustMessage);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _stationConsoles.Clear();
        _lastKnownJobs.Clear();
    }

    private void OnStartup(Entity<JobSlotsConsoleComponent> ent, ref ComponentStartup args)
    {
        // Try to find the station this console belongs to
        ent.Comp.Station = _station.GetOwningStation(ent);

        if (ent.Comp.Station != null)
        {
            if (!_stationConsoles.TryGetValue(ent.Comp.Station.Value, out var consoles))
            {
                consoles = [];
                _stationConsoles[ent.Comp.Station.Value] = consoles;
            }
            consoles.Add(ent);

            // Initialize last known jobs for this station
            if (!_lastKnownJobs.ContainsKey(ent.Comp.Station.Value))
            {
                var jobs = _jobs.GetJobs(ent.Comp.Station.Value);
                _lastKnownJobs[ent.Comp.Station.Value] = jobs.ToDictionary();
            }
        }

        UpdateUi(ent);
    }

    private void OnShutdown(Entity<JobSlotsConsoleComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.Station == null ||
            !_stationConsoles.TryGetValue(ent.Comp.Station.Value, out var consoles))
            return;

        consoles.Remove(ent);
        if (consoles.Count != 0)
            return;

        _stationConsoles.Remove(ent.Comp.Station.Value);
        _lastKnownJobs.Remove(ent.Comp.Station.Value);
    }

    private void OnInit(Entity<JobSlotsConsoleComponent> ent, ref ComponentInit args)
    {
        UpdateUi(ent);
    }

    private void OnUiOpened(Entity<JobSlotsConsoleComponent> ent, ref  BoundUIOpenedEvent args)
    {
        UpdateUi(ent);
    }

    private void OnAdjustMessage(Entity<JobSlotsConsoleComponent> ent, ref  JobSlotsConsoleAdjustMessage message)
    {
        if (ent.Comp.Station == null || !HasComp<StationJobsComponent>(ent.Comp.Station.Value))
            return;

        // Only update the UI if successful
        if (_jobs.TryAdjustJobSlot(ent.Comp.Station.Value, message.JobId, message.Adjustment, clamp: true))
            UpdateUi(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StationJobsComponent>();
        // This is a mess but it was the only way to update the UI when job slots change
        while (query.MoveNext(out var station, out _))
        {
            if (!_lastKnownJobs.TryGetValue(station, out var lastKnownJobs))
            {
                lastKnownJobs = new Dictionary<ProtoId<JobPrototype>, int?>();
                _lastKnownJobs[station] = lastKnownJobs;
            }

            var currentJobs = _jobs.GetJobs(station);
            var jobsChanged = false;

            // Check if any jobs changed
            if (currentJobs.Count != lastKnownJobs.Count)
            {
                jobsChanged = true;
            }
            else
            {
                foreach (var (jobId, slots) in currentJobs)
                {
                    if (lastKnownJobs.TryGetValue(jobId, out var oldSlots) && oldSlots == slots)
                        continue;

                    jobsChanged = true;
                    break;
                }
            }

            if (!jobsChanged)
                continue;

            // Update last known jobs
            _lastKnownJobs[station] = currentJobs.ToDictionary();
            UpdateStationConsoles(station);
        }
    }

    private void UpdateStationConsoles(EntityUid station)
    {
        if (!_stationConsoles.TryGetValue(station, out var consoles))
            return;

        foreach (var console in consoles)
        {
            if (!TryComp<JobSlotsConsoleComponent>(console, out var comp))
                continue;

            var ent = new Entity<JobSlotsConsoleComponent>(console, comp);
            UpdateUi(ent);
        }
    }


    private void UpdateUi(Entity<JobSlotsConsoleComponent> ent)
    {
        if (ent.Comp.Station is not { } station || !HasComp<StationJobsComponent>(station))
        {
            // If we don't have a valid station yet, send empty state
            var emptyState = new JobSlotsConsoleState(new Dictionary<ProtoId<JobPrototype>, int?>(), []);
            _ui.SetUiState(ent.Owner, JobSlotsConsoleUiKey.Key, emptyState);
            return;
        }

        var jobs = _jobs.GetJobs(station);
        var state = new JobSlotsConsoleState(jobs.ToDictionary(), ent.Comp.Blacklist);
        _ui.SetUiState(ent.Owner, JobSlotsConsoleUiKey.Key, state);
    }
}
