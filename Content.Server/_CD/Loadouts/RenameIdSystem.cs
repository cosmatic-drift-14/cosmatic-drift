using Content.Server.GameTicking;
using Content.Server.StationRecords.Systems;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Inventory;
using Content.Shared.PDA;
using Content.Shared.Roles.Jobs;

namespace Content.Server._CD.Loadouts;

public sealed class RenameIdSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedIdCardSystem _idCardSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // We need to be after station records system because otherwise we would remove the RenameIdComponent too early
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn, after: [ typeof(StationRecordsSystem) ]);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent args)
    {
        var player = args.Mob;
        if (!_inventorySystem.TryGetSlotEntity(player, "id", out var pdaUid))
            return;

        if (TryComp<PdaComponent>(pdaUid, out var pda)
            && TryComp<RenameIdComponent>(pdaUid, out var rename)
            && pda.ContainedId is {} id
            && TryComp<IdCardComponent>(id, out var card))
        {
            _idCardSystem.TryChangeJobTitle(id, Loc.GetString(rename.Value), card);
            RemComp(pdaUid.Value, rename);
        }
    }
}
