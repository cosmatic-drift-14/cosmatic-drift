using Content.Shared.Clothing;
using Content.Shared.Mind;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Station;
using Robust.Shared.Containers;

namespace Content.Shared._CD.Silicons;

public sealed class SiliconBrainLoadoutSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedStationSpawningSystem _station = default!;
    [Dependency] private readonly LoadoutSystem _loadout = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgChassisComponent, CdPlayerSpawnBeforeMindEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(Entity<BorgChassisComponent> ent, ref CdPlayerSpawnBeforeMindEvent args)
    {
        if (!_container.TryGetContainer(ent, ent.Comp.BrainContainerId, out var container))
            return;

        foreach (var brain in _container.EmptyContainer(container, true))
        {
            QueueDel(brain);
        }

        TrySpawnInContainer("MMI", ent, ent.Comp.BrainContainerId, out var mmi);
    }
}

/// <summary>
/// Event that raises right before the mind is added to the player entity while spawning on station.
/// </summary>
public sealed class CdPlayerSpawnBeforeMindEvent : EntityEventArgs;
