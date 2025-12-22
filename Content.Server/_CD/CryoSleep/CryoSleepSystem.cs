using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.EUI;
using Content.Server.Ghost;
using Content.Server.Hands.Systems;
using Content.Server.Mind;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
// using Content.Server._CD.Records;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.Climbing.Systems;
using Content.Shared.Database;
using Content.Shared.DragDrop;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Mobs.Components;
using Content.Shared.Roles.Jobs;
using Content.Shared.Verbs;
using Content.Shared._CD.CryoSleep;
using Content.Shared.StationRecords;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server._CD.CryoSleep;

public sealed class CryoSleepSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    // [Dependency] private readonly CharacterRecordsSystem _characterRecords = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly ClimbSystem _climb = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly EuiManager _eui = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly IAdminLogManager _adminLog = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly LostAndFoundSystem _lostAndFound = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly ISharedPlayerManager _player = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly StationJobsSystem _stationJobs = default!;
    [Dependency] private readonly StationRecordsSystem _records = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private EntityUid? _pausedMap;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CryoSleepComponent, ComponentInit>(ComponentInit);
        SubscribeLocalEvent<CryoSleepComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CryoSleepComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
        SubscribeLocalEvent<CryoSleepComponent, DragDropTargetEvent>(OnDragDrop);
    }

    private void ComponentInit(Entity<CryoSleepComponent> ent, ref ComponentInit args)
    {
        ent.Comp.BodyContainer = _container.EnsureContainer<ContainerSlot>(ent.Owner, "body_container");
    }

    private void OnShutdown(Entity<CryoSleepComponent> ent, ref ComponentShutdown args)
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

        var run = _mind.TryGetMind(toInsert.Value, out var mind, out var mindComp);
        _player.TryGetSessionById(mindComp?.UserId, out var session);
        if(run)
        {
            if (session?.Status == SessionStatus.Disconnected)
            {
                InsertBody(ent, toInsert.Value);
                return;
            }
        }

        var success = _container.Insert(toInsert.Value, ent.Comp.BodyContainer);

        if (success && _player.TryGetSessionById(mindComp?.UserId, out var updSession))
        {
            _eui.OpenEui(new CryoSleepEui(mind, this), updSession);
        }
    }

    public void CryoStoreBody(EntityUid mindId)
    {
        if (!_jobs.MindTryGetJob(mindId, out var prototype))
            return;

        if (!TryComp<MindComponent>(mindId, out var mind))
            return;

        var body = mind.CurrentEntity;
        var name = mind.CharacterName;

        if (body == null)
            return;

        if (!_player.TryGetSessionById(mind?.UserId, out var session))
            return;

        _adminLog.Add(LogType.Action,
            LogImpact.Low,
            $"Player {session} playing {ToPrettyString(body.Value)} entered cryosleep.");

        // Record items
        var foundItems = new List<LostItemData>();

        // Get inventory items
        var enumerator = _inventory.GetSlotEnumerator(body.Value);
        while (enumerator.NextItem(out var item, out var slotDef))
        {
            foundItems.Add(new LostItemData(
                slotDef.Name,
                MetaData(item).EntityName,
                GetNetEntity(item)
            ));
        }

        // Get held items
        foreach (var hand in _hands.EnumerateHands(body.Value))
        {
            if (!_hands.TryGetHeldItem(body.Value, hand, out var heldEntity))
                continue;

            foundItems.Add(new LostItemData(
                hand,
                MetaData(heldEntity.Value).EntityName,
                GetNetEntity(heldEntity.Value)
            ));
        }

        // Find Lost and Found locker and store items data
        var query = EntityQueryEnumerator<LostAndFoundComponent>();
        if (query.MoveNext(out var storage, out var lostAndFound))
        {
            var ent = (storage, lostAndFound);
            if (foundItems.Count > 0)
            {
                _lostAndFound.StorePlayerItems(ent, name!, foundItems); // best language
            }
        }

        // Remove records
        // TODO: cdrebase
        // if (TryComp<CharacterRecordKeyStorageComponent>(body, out var recordKey))
        // {
        //     _characterRecords.DeleteAllRecords(body.Value, recordKey);
        // }

        // Ensure nullspace map exists
        EnsurePausedMap();
        if (_pausedMap == null)
        {
            Log.Error("CryoSleep map was unexpectedly null");
            return;
        }

        // Ghost the player if needed and move body to nullspace
        _ghost.OnGhostAttempt(mindId, false, true, mind: mind);
        _transform.SetParent(body.Value, _pausedMap.Value);

        // Handle job slots and announcements
        if (!TryComp<MindComponent>(mindId, out var mindComp) || mindComp.UserId == null)
            return;

        foreach (var station in _station.GetStationsSet())
        {
            if (!TryComp<StationJobsComponent>(station, out var stationJobs))
               continue;

            if (!TryComp<StationRecordsComponent>(station, out var stationRecords))
                continue;

            if (!_stationJobs.TryGetPlayerJobs(station, mindComp.UserId.Value, out var jobs, stationJobs))
                continue;

            var recordId = _records.GetRecordByName(station, name!);
            if (recordId != null)
            {
                var key = new StationRecordKey(recordId.Value, station);
                if (_records.TryGetRecord<GeneralStationRecord>(key, out _, stationRecords))
                    _records.RemoveRecord(key, stationRecords);
            }

            foreach (var item in jobs)
            {
               _stationJobs.TryAdjustJobSlot(station, item, 1, clamp: true);
               _chat.DispatchStationAnnouncement(station,
                   Loc.GetString("cryo-leave-announcement",
                       ("character", name!),
                       ("job", prototype.LocalizedName)),
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
    private void EnsurePausedMap()
    {
        if (_pausedMap != null && Exists(_pausedMap))
            return;

        var mapUid = _map.CreateMap();
        _map.SetPaused(mapUid, true);
        _pausedMap = mapUid;
    }
}
