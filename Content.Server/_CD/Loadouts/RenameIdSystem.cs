using Content.Server.Access.Systems;
using Content.Server.GameTicking;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.PDA;

namespace Content.Server._CD.Loadouts;

public sealed class RenameIdSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly SharedIdCardSystem _idCardSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // We need to subscribe to both of these because RulePlayerJobsAssignedEvent only fires on round start and
        // messes up what we do in PlayerSpawnCompleteEvent
        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnJobsAssigned, after: [ typeof(PresetIdCardSystem) ]);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
    }

    private void OnJobsAssigned(RulePlayerJobsAssignedEvent args)
    {
        var query = EntityQuery<RenameIdComponent, PdaComponent>();
        foreach (var (rename, pda) in query)
        {
            if (pda.ContainedId is { } id
                && TryComp<IdCardComponent>(id, out var card))
            {
                _idCardSystem.TryChangeJobTitle(id, Loc.GetString(rename.Value), card);
            }
        }
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
        }
    }
}
