using Content.Server.GameTicking;
using Content.Server.StationRecords;
using Content.Shared._CD.Records;

namespace Content.Server._CD.Records;

public sealed class CharacterRecordsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        if (HasComp<StationRecordsComponent>(args.Station) && !HasComp<CharacterRecordsComponent>(args.Station))
            AddComp<CharacterRecordsComponent>(args.Station);

        if (args.Profile.CDCharacterRecords == null)
            return;

        AddRecord(args.Station, args.Mob, new CharacterRecords(args.Profile.CDCharacterRecords));

        // We don't delete records after a character has joined unless an admin requests it.
    }

    public bool AddRecord(EntityUid station, EntityUid player, CharacterRecords records, CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return false;

        recordsDb.Records.Add(player, records);

        return true;
    }

    // TODO: Admin tools for removing records
    public bool RemoveRecord(EntityUid station, EntityUid player, CharacterRecords records, CharacterRecordsComponent? recordsDb = null)
    {
        if (!Resolve(station, ref recordsDb))
            return false;

        return recordsDb.Records.Remove(player);
    }
}
