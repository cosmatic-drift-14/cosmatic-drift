using System.Diagnostics;
using Content.Shared.Clothing;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Silicons;

public sealed partial class SiliconBrainLoadoutSystem : EntitySystem
{
    [Dependency] private SharedContainerSystem _container = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    private ProtoId<LoadoutGroupPrototype> CyborgBrainLoadoutPrototype => "CyborgBrain";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BorgChassisComponent, CdPlayerSpawnBeforeMindEvent>(OnBorgPlayerSpawnComplete);
    }

    private void OnBorgPlayerSpawnComplete(Entity<BorgChassisComponent> ent, ref CdPlayerSpawnBeforeMindEvent args)
    {
        // surely there's a better way of doing this
        var jobLoadoutId = LoadoutSystem.GetJobPrototype(args.JobId);
        var selectedLoadouts = args
            .Character
            .GetLoadoutOrDefault(jobLoadoutId, args.Player, args.Character.Species, EntityManager, _proto)
            .SelectedLoadouts[CyborgBrainLoadoutPrototype];

        Debug.Assert(selectedLoadouts.Count == 1);

        var loadoutProto = selectedLoadouts[0].Prototype;



        if (!_container.TryGetContainer(ent, ent.Comp.BrainContainerId, out var container) ||
            !_proto.TryIndex(loadoutProto, out var loadout))
            return;

        // only run if we're absolutely SURE that we have a proto to replace it with
        foreach (var brain in _container.EmptyContainer(container))
        {
            QueueDel(brain);
        }

        TrySpawnInContainer(loadout.Brain, ent, ent.Comp.BrainContainerId, out var mmi);
    }
}

/// <summary>
/// Event that raises right before the mind is added to the player entity while spawning on station.
/// </summary>
public sealed record CdPlayerSpawnBeforeMindEvent(ICommonSession Player, HumanoidCharacterProfile Character, ProtoId<JobPrototype> JobId);
