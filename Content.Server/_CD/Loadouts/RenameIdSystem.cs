using Content.Server.Access.Systems;
using Content.Shared._CD.Loadouts;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.StationRecords;
using Content.Shared.StationRecords.Components;
using Content.Shared.StationRecords.Systems;

namespace Content.Server._CD.Loadouts;

public sealed partial class RenameIdSystem : SharedRenameIdSystem
{
    [Dependency] private InventorySystem _inventorySystem = default!;
    [Dependency] private SharedIdCardSystem _idCardSystem = default!;
    [Dependency] private StationRecordsSystem _records = default!;

    public override void Initialize()
    {
        base.Initialize();

        // We need to subscribe to both of these because RulePlayerJobsAssignedEvent only fires on round start and
        // messes up what we do in PlayerSpawnCompleteEvent
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnJobsAssigned, after: [ typeof(PresetIdCardSystem) ]);
    }

    private void OnJobsAssigned(RulePlayerJobsAssignedEvent args)
    {
        var query = EntityQuery<RenameIdComponent, PdaComponent>();
        foreach (var (rename, pda) in query)
        {
            if (pda.ContainedId is { } id
                && TryComp<IdCardComponent>(id, out var card))
            {
                if (rename.NewIcon != null)
                {
                    card.JobIcon = rename.NewIcon; // TryChangeJobTitle dirties the ID for us
                    FlushIdIconToRecords(id, rename.NewIcon);
                }
                _idCardSystem.TryChangeJobTitle(id, Loc.GetString(rename.Value), card);
            }
        }
    }

    // Needed to update the icon on the manifest
    // why do I need to do this
    private void FlushIdIconToRecords(EntityUid targetId, string newJobIcon)
    {
        if (!TryComp<StationRecordKeyStorageComponent>(targetId, out var keyStorage)
            || keyStorage.Key is not { } key
            || !_records.TryGetRecord<GeneralStationRecord>(key, out var record))
        {
            return;
        }

        record.JobIcon = newJobIcon;

        _records.Synchronize(key);
    }
}
