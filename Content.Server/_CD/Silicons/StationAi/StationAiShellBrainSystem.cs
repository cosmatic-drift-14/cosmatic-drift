using Content.Server.Mind;
using Content.Shared._CD.Silicons.StationAi;
using Content.Shared.NameModifier.Components;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Silicons.StationAi;

public sealed class StationAiShellBrainSystem : EntitySystem
{
    [Dependency] private readonly StationAiShellUserSystem _shelluser = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiShellBrainComponent, EntGotInsertedIntoContainerMessage>(OnShellInsert);
        SubscribeLocalEvent<StationAiShellBrainComponent, EntGotRemovedFromContainerMessage>(OnShellEject);
    }

    private void OnShellInsert(Entity<StationAiShellBrainComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (!TryComp<BorgChassisComponent>(args.Container.Owner, out var chassis))
            return;

        var query = EntityQueryEnumerator<StationAiShellUserComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            _shelluser.AddToAvailableShells((uid, comp), args.Container.Owner!);
        }

        _shelluser.ExitShell(ent.Owner);
        ent.Comp.ContainingShell = args.Container.Owner;
        SetShellName(ent.Owner);
        Log.Debug("    PASS - BORIS INSERT DETECTED");
    }

    private void OnShellEject(Entity<StationAiShellBrainComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (!TryComp<BorgChassisComponent>(args.Container.Owner, out var chassis))
            return;

        var query = EntityQueryEnumerator<StationAiShellUserComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            _shelluser.RemoveFromAvailableShells((uid, comp), args.Container.Owner!);
        }

        Name(ent);
        Log.Debug("    PASS - BORIS EXIT DETECTED");
    }

    /// <summary>
    /// Automatically sets the name of the given brain's shell to the appropriate format
    /// </summary>
    /// <param name="shellBrain">The brain of the shell we want to set the name of</param>
    public void SetShellName(Entity<StationAiShellBrainComponent?> shellBrain)
    {
        if (!Resolve(shellBrain, ref shellBrain.Comp))
            return;

        if (!TryComp<BorgSwitchableTypeComponent>(shellBrain.Comp.ContainingShell, out var switchable))
            return;

        string formattedName;
        if (shellBrain.Comp.ActiveCore == null)
        {
            formattedName = Loc.GetString("empty-shell");
        }
        else
        {
            _prototype.TryIndex(switchable.SelectedBorgType, out var borgProto);
            var borgTypeLoc = Loc.GetString($"borg-type-{borgProto?.ID ?? "default"}-name");

            var aiName = TryComp<NameModifierComponent>(shellBrain.Comp.ActiveCore.Value, out var nameModifier)
                ? nameModifier.BaseName
                : Loc.GetString("shell-ai-name-not-found");

            formattedName = Loc.GetString("shell-name-format",
                ("name", aiName),
                ("shellType", borgTypeLoc));
        }

        _metaData.SetEntityName(shellBrain.Comp.ContainingShell.Value, formattedName);
    }
}
