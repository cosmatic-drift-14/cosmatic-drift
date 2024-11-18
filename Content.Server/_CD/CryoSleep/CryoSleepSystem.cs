using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.EUI;
using Content.Server.Forensics;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Server.StationRecords.Systems;
using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Server._CD.Records;
using Content.Server._CD.Storage.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Climbing.Systems;
using Content.Shared.Database;
using Content.Shared.Destructible;
using Content.Shared.DragDrop;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.PDA;
using Content.Shared.Roles.Jobs;
using Content.Shared.StationRecords;
using Content.Shared.Verbs;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Enums;

namespace Content.Server._CD.CryoSleep;

public sealed class CryoSleepSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly CharacterRecordsSystem _characterRecords = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ClimbSystem _climb = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly EuiManager _eui = null!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CryoSleepComponent, ComponentInit>(ComponentInit);
        SubscribeLocalEvent<CryoSleepComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
        SubscribeLocalEvent<CryoSleepComponent, DestructionEventArgs>(OnDestruction);
        SubscribeLocalEvent<CryoSleepComponent, DragDropTargetEvent>(OnDragDrop);
    }

    private void ComponentInit(Entity<CryoSleepComponent> ent, ref ComponentInit args)
    {
        ent.Comp.BodyContainer = _container.EnsureContainer<ContainerSlot>(ent.Owner, "body_container");
    }

    private void OnDestruction(Entity<CryoSleepComponent> ent, ref DestructionEventArgs args)
    {
        EjectBody(ent);
    }

    private void InsertBody(Entity<CryoSleepComponent> ent, EntityUid? toInsert)
    {
        if (toInsert == null || IsOccupied(ent))
            return;

        if (!HasComp<MobStateComponent>(toInsert.Value))
            return;

        _container.Insert(toInsert.Value, ent.Comp.BodyContainer);
    }

    private void RespawnUser(Entity<CryoSleepComponent> ent, EntityUid? toInsert, bool force)
    {
        if (toInsert == null)
            return;

        if (IsOccupied(ent) && !force)
            return;

        if (_mind.TryGetMind(toInsert.Value, out var mind, out var mindComp))
        {
            var session = mindComp.Session;
            if (session != null && session.Status == SessionStatus.Disconnected)
            {
                InsertBody(ent, toInsert.Value);
                return;
            }
        }

        var success = _container.Insert(toInsert.Value, ent.Comp.BodyContainer);

        if (success && mindComp?.Session != null)
        {
            _eui.OpenEui(new CryoSleepEui(mind, this), mindComp.Session);
        }
    }

    public void CryoStoreBody(EntityUid mindId)
    {
        if (!_jobs.MindTryGetJob(mindId, out var prototype))
            return;

        if (!TryComp<MindComponent>(mindId, out var mind))
            return;

        var body = mind.CurrentEntity;
        var job = prototype;

        var name = mind.CharacterName;

        if (body == null)
            return;

        _adminLog.Add(LogType.Respawn, LogImpact.Low, $"Player {mind.Session} playing {ToPrettyString(body)} entered cryosleep.");

        // Remove the record. Hopefully.
        foreach (var item in _inventory.GetHandOrInventoryEntities(body.Value))
        {
            if (!TryComp(item, out PdaComponent? pda) ||
                !TryComp(pda.ContainedId, out StationRecordKeyStorageComponent? keyStorage) ||
                keyStorage.Key is not { } key || !_stationRecords.TryGetRecord(key, out GeneralStationRecord? record))
                continue;

            if (TryComp(body, out DnaComponent? dna) &&
                dna.DNA != record.DNA)
                continue;

            if (TryComp(body, out FingerprintComponent? fingerPrint) &&
                fingerPrint.Fingerprint != record.Fingerprint)
                continue;

            _stationRecords.RemoveRecord(key);
            Del(item);
        }

        if (TryComp<CharacterRecordKeyStorageComponent>(body, out var recordKey))
        {
            _characterRecords.DeleteAllRecords(body.Value, recordKey);
        }

        // Move their items
        MoveItems(body.Value);

        _ghost.OnGhostAttempt(mindId, false, true, mind: mind);
        EntityManager.DeleteEntity(body);

        if (!TryComp<MindComponent>(mindId, out var mindComp) || mindComp.UserId == null)
            return;

        foreach (var station in _station.GetStationsSet())
        {
            if (!TryComp<StationJobsComponent>(station, out var stationJobs))
               continue;

            if (!_stationJobs.TryGetPlayerJobs(station, mindComp.UserId.Value, out var jobs, stationJobs))
                continue;

            foreach (var item in jobs)
            {
               _stationJobs.TryAdjustJobSlot(station, item, 1, clamp: true);
               _chat.DispatchStationAnnouncement(station,
                   Loc.GetString("cryo-leave-announcement",
                       ("character", name!),
                       ("job", job.LocalizedName)),
                   "Cryo Pod",
                   false);
            }

            _stationJobs.TryRemovePlayerJobs(station, mindComp.UserId.Value, stationJobs);
        }
    }

    private void EjectBody(Entity<CryoSleepComponent> ent)
    {
        if (!IsOccupied(ent))
            return;

        var toEject = ent.Comp.BodyContainer.ContainedEntity;
        if (toEject == null)
            return;

        _container.Remove(toEject.Value, ent.Comp.BodyContainer);
        _climb.ForciblySetClimbing(toEject.Value, ent.Owner);
    }

    private static bool IsOccupied(Entity<CryoSleepComponent> ent)
    {
        return ent.Comp.BodyContainer.ContainedEntity != null;
    }

    private void AddAlternativeVerbs(Entity<CryoSleepComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Insert self verb
        if (!IsOccupied(ent) &&
            _blocker.CanMove(args.User))
        {
            var user = args.User;
            AlternativeVerb verb = new()
            {
                Act = () => RespawnUser(ent, user, false),
                Category = VerbCategory.Insert,
                Text = Loc.GetString("medical-scanner-verb-enter")
            };
            args.Verbs.Add(verb);
        }

        // Eject somebody verb
        if (IsOccupied(ent))
        {
            AlternativeVerb verb = new()
            {
                Act = () => EjectBody(ent),
                Category = VerbCategory.Eject,
                Text = Loc.GetString("medical-scanner-verb-noun-occupant")
            };
            args.Verbs.Add(verb);
        }
    }

    private void OnDragDrop(Entity<CryoSleepComponent> ent, ref DragDropTargetEvent args)
    {
        if (args.Handled || args.User != args.Dragged)
            return;

        RespawnUser(ent, args.User, false);
    }

    private void MoveItems(EntityUid uid)
    {
        var query = EntityQueryEnumerator<LostAndFoundComponent>();
        query.MoveNext(out var locker, out _);

        // Make sure the locker exists and has storage
        if (!locker.Valid)
            return;

        TryComp<EntityStorageComponent>(uid, out var lockerStorageComp);

        var coordinates = Transform(locker).Coordinates;

        // Go through their inventory and put everything in a locker
        foreach (var item in _inventory.GetHandOrInventoryEntities(uid))
        {
            if (!item.IsValid() || !TryComp<MetaDataComponent>(item, out var comp))
                continue;

            var proto = comp.EntityPrototype;
            var ent = EntityManager.SpawnEntity(proto!.ID, coordinates);

            _entityStorage.Insert(ent, locker, lockerStorageComp);
        }
    }
}
