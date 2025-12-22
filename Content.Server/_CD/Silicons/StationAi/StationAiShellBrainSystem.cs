using Content.Server.Mind;
using Content.Shared._CD.Silicons.StationAi;
using Content.Shared.NameModifier.Components;
using Content.Shared.Silicons.Borgs;
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

        Subs.BuiEvents<StationAiShellBrainHolderComponent>(BorgSwitchableTypeUiKey.SelectBorgType,
            subs =>
            {
                subs.Event<BorgSelectTypeMessage>(OnSelectShellType);
            });
    }

    private void OnSelectShellType(EntityUid uid, StationAiShellBrainHolderComponent component, BorgSelectTypeMessage args)
    {
        SetShellName(component.Brain, args.Prototype);
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

        ent.Comp.ContainingShell = args.Container.Owner;
        SetShellName(ent.Owner);
        var brainHolderComp = AddComp<StationAiShellBrainHolderComponent>(ent.Comp.ContainingShell.Value);
        brainHolderComp.Brain = ent;
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

        _shelluser.ExitShell(ent.Owner);
        if (ent.Comp.ContainingShell != null)
            RemCompDeferred<StationAiShellBrainHolderComponent>(ent.Comp.ContainingShell.Value);
        Name(ent);
    }

    /// <summary>
    /// Automatically sets the name of the given brain's shell to the appropriate format
    /// </summary>
    /// <param name="shellBrain">The brain of the shell we want to set the name of</param>
    /// <param name="borgProtoId">The protoId of the borg type for our name. Primarily used for setting our name on borg type selection</param>
    public void SetShellName(Entity<StationAiShellBrainComponent?> shellBrain, ProtoId<BorgTypePrototype>? borgProtoId = null)
    {
        if (!Resolve(shellBrain, ref shellBrain.Comp))
            return;

        if (!TryComp<BorgSwitchableTypeComponent>(shellBrain.Comp.ContainingShell, out var switchable))
            return;

        string formattedName;
        if (shellBrain.Comp.ActiveCore == null)
        {
            formattedName = Loc.GetString("cd-ai-shell-brain-empty");
        }
        else
        {
            borgProtoId ??= switchable.SelectedBorgType;
            _prototype.TryIndex(borgProtoId, out var borgProto);

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
