using Content.Shared.Containers.ItemSlots;
using Content.Shared.Gibbing.Events;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Containers;

namespace Content.Shared._CD.Silicons.StationAi;

public abstract class SharedStationAiShellBrainSystem : EntitySystem
{
    [Dependency] private readonly SharedStationAiShellUserSystem _shellUser = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiShellBrainComponent, EntGotInsertedIntoContainerMessage>(OnShellInsert);
        SubscribeLocalEvent<StationAiShellBrainComponent, EntGotRemovedFromContainerMessage>(OnShellExit);

    }

    protected virtual void OnShellInsert(Entity<StationAiShellBrainComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!TryComp<BorgChassisComponent>(args.Container.Owner, out var chassis))
            return;

        Log.Debug("    PASS - BORIS INSERT DETECTED");
    }

    protected virtual void OnShellExit(Entity<StationAiShellBrainComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<BorgChassisComponent>(args.Container.Owner, out var chassis))
            return;

        // Make sure we have an AI to delink ourselves from in the first place
        //if (!TryComp<StationAiShellUserComponent>(ent.Comp.ActiveCore, out var shellUser))
        //    return;

        Log.Debug("    PASS - BORIS EXIT DETECTED");
    }

}
