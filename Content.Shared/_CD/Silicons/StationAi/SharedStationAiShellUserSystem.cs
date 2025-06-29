using System.Runtime.InteropServices;
using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Movement.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.StationAi;
using Microsoft.VisualBasic.CompilerServices;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.Silicons.StationAi;

public abstract class SharedStationAiShellUserSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedStationAiSystem _stationAiSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UserInterface = default!;
    [Dependency] private readonly EntityManager _entity = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiShellUserComponent, AiEnterShellEvent>(OnOpenUi);

        Subs.BuiEvents<StationAiShellUserComponent>(ShellUiKey.Key,
            subs =>
            {
                subs.Event<EnterShellMessage>(OnEnterShell);
                subs.Event<JumpToShellMessage>(OnJumpToShell);
            });

        SubscribeLocalEvent<BorgChassisComponent, AiExitShellEvent>(OnExitShell);

        SubscribeLocalEvent<StationAiShellUserComponent, IonStormLawsEvent>(OnIonStormLaws);
    }

    private void OnJumpToShell(EntityUid uid, StationAiShellUserComponent component, JumpToShellMessage args)
    {
        if(!_stationAiSystem.TryGetCore(uid, out var core))
            return;

        if (!_entity.TryGetEntity(args.Shell, out var shellEnt))
            return;

        if (core.Comp == null)
            return;

        if (core.Comp.RemoteEntity.HasValue)
            _xforms.DropNextTo(core.Comp.RemoteEntity.Value, shellEnt.Value);

        Log.Debug($"Jumped to shell {uid}");
    }

    private void OnOpenUi(Entity<StationAiShellUserComponent> ent, ref AiEnterShellEvent args)
    {
        UserInterface.TryToggleUi(args.Performer, ShellUiKey.Key, ent);
    }

    private void OnEnterShell(Entity<StationAiShellUserComponent> ent, ref EnterShellMessage args)
    {
        if (!TryComp<StationAiHeldComponent>(ent.Owner, out var held)) // Check that the user is an AI
            return;

        if (!_stationAiSystem.TryGetCore(ent.Owner, out var core)) // And check that they have a core
            return;


        // Now, select a shell
        if (_net.IsClient)
            return;

        if (args.Shell == null)
            return;

        var shellEnt = _entity.GetEntity(args.Shell); // Manual conversion to EntityUid because of UI bullshit (doesn't automatically convert and can't send uids over the network)
        if (!TryComp<BorgChassisComponent>(shellEnt, out var chassis))
            return;

        // First make sure the selected chassis has a brain, and it's a BORIS module
        if (!chassis.BrainEntity.HasValue &&
            !HasComp<StationAiShellBrainComponent>(chassis.BrainEntity))
            return;

        ent.Comp.SelectedShell = shellEnt;
        ent.Comp.SelectedBrain = chassis.BrainEntity;


        // Anything below this would be the "on possess" button being pressed
        if (!_mind.TryGetMind(ent.Owner, out var mindId, out var mind)) // Then get the AI's mind
            return;

        if (!ent.Comp.SelectedBrain.HasValue || !ent.Comp.SelectedShell.HasValue ||
            !TryComp<StationAiShellBrainComponent>(ent.Comp.SelectedBrain.Value,
                out var shellBrain)) // Get the brain of the shell
            return;

        if (core.Comp == null)
            return;

        shellBrain.ActiveCore = ent.Owner;
        _mind.TransferTo(mindId, ent.Comp.SelectedShell, mind: mind);
        _actions.AddAction(ent.Comp.SelectedShell.Value, ref ent.Comp.ActionEntity, ent.Comp.ActionPrototype);
        RemCompDeferred<IonStormTargetComponent>(ent.Comp.SelectedShell.Value);

        // Put the eye at the core
        if (core.Comp.RemoteEntity.HasValue)
            _xforms.DropNextTo(core.Comp.RemoteEntity.Value, core.Owner);

        // Set the chassis' name to the AI's
        var metaData = MetaData(ent.Owner);
        _metaData.SetEntityName(ent.Comp.SelectedShell.Value, metaData.EntityName);

        // Add AI radio channels to the chassis
        AddChannels(ent.Comp.SelectedShell.Value, ent.Owner);
    }

    private void OnExitShell(Entity<BorgChassisComponent> ent, ref AiExitShellEvent args)
    {
        if (!_mind.TryGetMind(ent.Owner, out var mindId, out var mind)) // First get our brain
            return;

        if (!TryComp<StationAiShellBrainComponent>(ent.Comp.BrainEntity, out var shellBrain))
            return;

        var brainUid = shellBrain.ActiveCore;
        if (!brainUid.HasValue ||
            !TryComp<StationAiHeldComponent>(brainUid, out var held)) // Check that the user is an AI
            return;

        if (!_stationAiSystem.TryGetCore(brainUid.Value, out var core)) // And check that they have a core
            return;

        if (!TryComp<StationAiShellUserComponent>(brainUid, out var shellUser))
            return;

        _mind.TransferTo(mindId, brainUid, mind: mind);
        _stationAiSystem.SetupEye(core!);
        _stationAiSystem.AttachEye(core!);

        var test = (brainUid, shellUser);

        RemoveChannels(ent.Owner, brainUid.Value);
        _actions.RemoveAction(shellUser.ActionEntity);
        shellUser.ActionEntity = null;
        AddComp<IonStormTargetComponent>(ent);

        if (core.Comp == null)
            return;

        if (core.Comp.RemoteEntity.HasValue)
            _xforms.DropNextTo(core.Comp.RemoteEntity.Value, ent.Owner);
    }

    private void OnIonStormLaws(Entity<StationAiShellUserComponent> ent, ref IonStormLawsEvent args)
    {
        ChangeShellLaws(ent, args.Lawset);
    }

    public virtual void ChangeShellLaws(EntityUid entity, SiliconLawset lawset)
    {
    }

    protected virtual void AddChannels(Entity<BorgChassisComponent?> chassis,
        Entity<StationAiShellUserComponent?> shellUser)
    {
    }

    protected virtual void RemoveChannels(Entity<BorgChassisComponent?> chassis, Entity<StationAiHeldComponent?> held)
    {
    }
}

public sealed partial class AiEnterShellEvent : InstantActionEvent
{
}

public sealed partial class AiExitShellEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public enum ShellUiKey : byte
{
    Key
}
