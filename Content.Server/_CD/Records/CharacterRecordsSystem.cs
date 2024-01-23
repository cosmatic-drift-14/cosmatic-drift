using Content.Server.Forensics;
using Content.Server.GameTicking;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared._CD.Records;
using Content.Shared.Inventory;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Records;

public sealed class CharacterRecordsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        if (HasComp<StationRecordsComponent>(args.Station) && !HasComp<CharacterRecordsComponent>(args.Station))
            AddComp<CharacterRecordsComponent>(args.Station);

        var profile = args.Profile;
        if (profile.CDCharacterRecords == null || string.IsNullOrEmpty(args.JobId))
            return;

        var player = args.Mob;

        if (!_inventorySystem.TryGetSlotEntity(player, "id", out var idUid))
        {
            return;
        }

        if (!_prototypeManager.TryIndex(args.JobId, out JobPrototype? jobPrototype))
        {
            throw new ArgumentException($"Invalid job prototype ID: {args.JobId}");
        }

        TryComp<FingerprintComponent>(player, out var fingerprintComponent);
        TryComp<DnaComponent>(player, out var dnaComponent);

        var records = new FullCharacterRecords(
            characterRecords: new CharacterRecords(profile.CDCharacterRecords),
            name: profile.Name,
            age: profile.Age,
            species: profile.Species,
            jobTitle: jobPrototype.LocalizedName,
            jobIcon: jobPrototype.Icon,
            gender: profile.Gender,
            sex: profile.Sex,
            fingerprint: fingerprintComponent?.Fingerprint,
            dna: dnaComponent?.DNA);
        AddRecord(args.Station, args.Mob, records);

        RaiseLocalEvent(args.Station, new CharacterRecordsModifiedEvent(args.Mob, records));

        // We don't delete records after a character has joined unless an admin requests it.
    }

    public bool AddRecord(EntityUid station, EntityUid player, FullCharacterRecords records, CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return false;

        recordsDb.Records.Add(player, records);

        return true;
    }

    // TODO: Admin tools for removing records
    public bool RemoveRecord(EntityUid station, EntityUid player, CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return false;

        return recordsDb.Records.Remove(player);
    }

    public IDictionary<EntityUid, FullCharacterRecords> QueryRecords(EntityUid station, CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return new Dictionary<EntityUid, FullCharacterRecords>();

        return recordsDb.Records;
    }
}

public sealed class CharacterRecordsModifiedEvent : EntityEventArgs
{
    public EntityUid Player;
    public FullCharacterRecords NewRecords;

    public CharacterRecordsModifiedEvent(EntityUid player, FullCharacterRecords newRecords)
    {
        Player = player;
        NewRecords = newRecords;
    }
}
