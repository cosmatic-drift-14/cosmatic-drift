using Content.Server.Forensics;
using Content.Server.GameTicking;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared._CD.Records;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Roles;
using Content.Shared.StationRecords;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Records;

public sealed class CharacterRecordsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn, after: new []{ typeof(StationRecordsSystem) });
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        if (!HasComp<StationRecordsComponent>(args.Station))
        {
            Log.Error("Tried to add CharacterRecords on a station without StationRecords");
            return;
        }
        if (!HasComp<CharacterRecordsComponent>(args.Station))
            AddComp<CharacterRecordsComponent>(args.Station);

        var profile = args.Profile;
        if (profile.CDCharacterRecords == null || string.IsNullOrEmpty(args.JobId))
            return;

        var player = args.Mob;

        if (!_prototypeManager.TryIndex(args.JobId, out JobPrototype? jobPrototype))
        {
            throw new ArgumentException($"Invalid job prototype ID: {args.JobId}");
        }

        TryComp<FingerprintComponent>(player, out var fingerprintComponent);
        TryComp<DnaComponent>(player, out var dnaComponent);

        var records = new FullCharacterRecords(
            characterRecords: new CharacterRecords(profile.CDCharacterRecords),
            stationRecordsKey: FindStationRecordsKey(player),
            name: profile.Name,
            age: profile.Age,
            species: profile.Species,
            jobTitle: jobPrototype.LocalizedName,
            jobIcon: jobPrototype.Icon,
            gender: profile.Gender,
            sex: profile.Sex,
            fingerprint: fingerprintComponent?.Fingerprint,
            dna: dnaComponent?.DNA,
            owner: player);
        AddRecord(args.Station, args.Mob, records);
    }

    private uint? FindStationRecordsKey(EntityUid uid)
    {
        if (!_inventorySystem.TryGetSlotEntity(uid, "id", out var idUid))
            return null;

        var keyStorageEntity = idUid;
        if (TryComp<PdaComponent>(idUid, out var pda) && pda.ContainedId is {} id)
        {
            keyStorageEntity = id;
        }

        if (!TryComp<StationRecordKeyStorageComponent>(keyStorageEntity, out var storage))
        {
            return null;
        }

        return storage.Key?.Id;
    }

    private void AddRecord(EntityUid station, EntityUid player, FullCharacterRecords records, CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return;

        uint key = recordsDb.CreateNewKey();
        recordsDb.Records.Add(key, records);
        var playerKey = new CharacterRecordKey { Station = station, Index = key };
        AddComp(player, new CharacterRecordKeyStorageComponent(playerKey));

        RaiseLocalEvent(station, new CharacterRecordsModifiedEvent());
    }

    public void DelEntry(
        EntityUid station,
        EntityUid player,
        CharacterRecordType ty,
        int idx,
        CharacterRecordsComponent? recordsDb = null,
        CharacterRecordKeyStorageComponent? key = null)
    {
        if (!Resolve(station, ref recordsDb) || !Resolve(player, ref key))
            return;

        if (!recordsDb.Records.ContainsKey(key.Key.Index))
            return;

        var cr = recordsDb.Records[key.Key.Index].CharacterRecords;

        switch (ty)
        {
            case CharacterRecordType.Employment:
                cr.EmploymentEntries.RemoveAt(idx);
                break;
            case CharacterRecordType.Medical:
                cr.MedicalEntries.RemoveAt(idx);
                break;
            case CharacterRecordType.Security:
                cr.SecurityEntries.RemoveAt(idx);
                break;
        }

        RaiseLocalEvent(station, new CharacterRecordsModifiedEvent());
    }

    public void ResetRecord(
        EntityUid station,
        EntityUid player,
        CharacterRecordsComponent? recordsDb = null,
        CharacterRecordKeyStorageComponent? key = null)
    {
        if (!Resolve(station, ref recordsDb) || !Resolve(player, ref key))
            return;

        if (!recordsDb.Records.ContainsKey(key.Key.Index))
            return;

        var records = CharacterRecords.DefaultRecords();
        recordsDb.Records[key.Key.Index].CharacterRecords = records;
        RaiseLocalEvent(station, new CharacterRecordsModifiedEvent());
    }

    public void DeleteAllRecords(EntityUid player, CharacterRecordKeyStorageComponent? key = null)
    {
        if (!Resolve(player, ref key))
            return;

        var station = key.Key.Station;
        CharacterRecordsComponent? records = null;
        if (!Resolve(station, ref records))
            return;

        records.Records.Remove(key.Key.Index);
    }

    public IDictionary<uint, FullCharacterRecords> QueryRecords(EntityUid station, CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return new Dictionary<uint, FullCharacterRecords>();

        return recordsDb.Records;
    }
}

public sealed class CharacterRecordsModifiedEvent : EntityEventArgs
{

    public CharacterRecordsModifiedEvent()
    {
    }
}
