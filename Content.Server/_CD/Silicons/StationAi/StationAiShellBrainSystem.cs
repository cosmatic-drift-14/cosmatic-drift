using Content.Server.Mind;
using Content.Shared._CD.Silicons.StationAi;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._CD.Silicons.StationAi;

public sealed class StationAiShellBrainSystem : SharedStationAiShellBrainSystem
{
    [Dependency] private readonly StationAiShellUserSystem _shelluser = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiShellBrainComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<StationAiShellBrainComponent> ent, ref MapInitEvent args)
    {
        if (!_container.TryGetContainingContainer(ent.Owner, out var container))
            return;

        MetaDataSystem.SetEntityName(container.Owner, "HELLO WORLD:");
    }

    protected override void OnShellInsert(Entity<StationAiShellBrainComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!TryComp<BorgChassisComponent>(args.Container.Owner, out var chassis))
            return;

        foreach (var shellUser in EntityQuery<StationAiShellUserComponent>())
        {
                _shelluser.AddToAvailableShells(shellUser, args.Container.Owner!);
        }

        ent.Comp.ContainingShell = args.Container.Owner;
        SetShellName(ent.Owner);
        Log.Debug("    PASS - BORIS INSERT DETECTED");
    }

    protected override void OnShellEject(Entity<StationAiShellBrainComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<BorgChassisComponent>(args.Container.Owner, out var chassis))
            return;

        foreach (var shellUser in EntityQuery<StationAiShellUserComponent>())
        {
            _shelluser.RemoveFromAvailableShells(shellUser, args.Container.Owner!);
        }

        _shelluser.ExitShell(ent.Owner);

        // Make sure we have an AI to delink ourselves from in the first place
        //if (!TryComp<StationAiShellUserComponent>(ent.Comp.ActiveCore, out var shellUser))
        //    return;

        Name(ent);
        Log.Debug("    PASS - BORIS EXIT DETECTED");
    }

}
