using Content.Shared.Containers.ItemSlots;
using Content.Shared.Gibbing.Events;
using Content.Shared.NameModifier.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._CD.Silicons.StationAi;


// TODO: just bring this all into server ffs
public abstract class SharedStationAiShellBrainSystem : EntitySystem
{
    [Dependency] private readonly SharedStationAiShellUserSystem _shellUser = default!;
    [Dependency] protected readonly MetaDataSystem MetaDataSystem = default!;
    [Dependency] private readonly SharedStationAiSystem  _stationAi = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

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

        ent.Comp.ContainingShell = args.Container.Owner;
        SetShellName(ent);
        Log.Debug("    PASS - BORIS INSERT DETECTED");
    }

    protected virtual void OnShellExit(Entity<StationAiShellBrainComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<BorgChassisComponent>(args.Container.Owner, out var chassis))
            return;

        ent.Comp.ContainingShell = null;

        Log.Debug("    PASS - BORIS EXIT DETECTED");
    }

    // TODO: fix this up and make sure it uses localization, probably will make things a ton easier
    public void SetShellName(Entity<StationAiShellBrainComponent> shellBrain)
    {
        if (shellBrain.Comp == null)
            return;

        var shell = shellBrain.Comp.ContainingShell;

        if (!TryComp<BorgChassisComponent>(shell, out var chassis))
            return;

        if (!TryComp<BorgSwitchableTypeComponent>(shell, out var switchable))
            return;

        string formattedName;
        if (shellBrain.Comp.ActiveCore != null)
        {
            TryComp<NameModifierComponent>(shellBrain.Comp.ActiveCore.Value, out var nameModifier);

            _prototype.TryIndex(switchable.SelectedBorgType, out var borgProto);

            formattedName = $"{nameModifier?.BaseName ?? "NOT-FOUND"} {borgProto?.ID.ToUpperInvariant() ?? "Default"} Shell";
        }
        else
            formattedName = "Empty Default Shell";

        if (shellBrain.Comp.ContainingShell != null)
            MetaDataSystem.SetEntityName(shellBrain.Comp.ContainingShell.Value, formattedName);
    }

}
