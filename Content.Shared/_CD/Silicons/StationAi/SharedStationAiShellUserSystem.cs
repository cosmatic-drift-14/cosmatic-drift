using System.Runtime.InteropServices;
using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Movement.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.StationAi;
using Microsoft.VisualBasic.CompilerServices;
using Robust.Shared.Audio;
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
        SubscribeLocalEvent<BorgChassisComponent, AiExitShellEvent>(OnExitShell);

        SubscribeLocalEvent<StationAiShellUserComponent, IonStormLawsEvent>(OnIonStormLaws);

        Subs.BuiEvents<StationAiShellUserComponent>(ShellUiKey.Key,
            subs =>
            {
                subs.Event<JumpToShellMessage>(OnJumpToShell);
                subs.Event<EnterShellMessage>(OnEnterShell);
                subs.Event<SelectShellMessage>(OnSelectShell);
            });
    }

    private void OnOpenUi(Entity<StationAiShellUserComponent> ent, ref AiEnterShellEvent args)
    {
        UserInterface.TryToggleUi(args.Performer, ShellUiKey.Key, ent);
    }

    private void OnSelectShell(EntityUid uid, StationAiShellUserComponent component, SelectShellMessage args)
    {
        if (_net.IsClient)
            return;

        var shellEnt = _entity.GetEntity(args.Shell); // Manual conversion to EntityUid because of UI bullshit (doesn't automatically convert and can't send uids over the network)
        if (!TryComp<BorgChassisComponent>(shellEnt, out var chassis))
            return;

        // Make sure the selected chassis has a brain, and it's a BORIS module
        if (!chassis.BrainEntity.HasValue &&
            !HasComp<StationAiShellBrainComponent>(chassis.BrainEntity))
            return;

        component.SelectedShell = shellEnt;
        component.SelectedBrain = chassis.BrainEntity;
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


    protected virtual void OnEnterShell(Entity<StationAiShellUserComponent> ent, ref EnterShellMessage args)
    {
        if (!_stationAiSystem.TryGetCore(ent, out var core)) // Check that the user is an AI with a core
            return;

        if (!_mind.TryGetMind(ent.Owner, out var mindId, out var mind)) // Then get the AI's mind
            return;

        if (!ent.Comp.SelectedBrain.HasValue || !ent.Comp.SelectedShell.HasValue ||
            !TryComp<StationAiShellBrainComponent>(ent.Comp.SelectedBrain.Value,
                out var shellBrain))
            return;

        if (core.Comp == null)
            return;

        shellBrain.ActiveCore = ent.Owner;
        if(TryComp<SiliconLawProviderComponent>(ent.Owner, out var lawProvider))
            ChangeShellLaws(ent.Owner, lawProvider.Lawset);
        _actions.AddAction(ent.Comp.SelectedShell.Value, ref ent.Comp.ActionEntity, ent.Comp.ActionPrototype);
        RemCompDeferred<IonStormTargetComponent>(ent.Comp.SelectedShell.Value);

        _stationAiSystem.SwitchRemoteEntityMode(core, false);
        _mind.TransferTo(mindId, ent.Comp.SelectedShell, mind: mind);

        // Add AI radio channels to the chassis
        AddChannels(ent.Comp.SelectedShell.Value, ent);

        Dirty(ent); // icky networking
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

        _actions.RemoveAction(shellUser.ActionEntity);
        RemoveChannels(ent.Owner, brainUid.Value);
        AddComp<IonStormTargetComponent>(ent);

        shellUser.ActionEntity = null;
        ExitShell(ent.Comp.BrainEntity.Value, mind: (mindId, mind));

        if (core.Comp == null)
            return;

        if (core.Comp.RemoteEntity.HasValue)
            _xforms.DropNextTo(core.Comp.RemoteEntity.Value, ent.Owner);
    }

    /// <summary>
    /// The method to call when we want to exit the given shell
    /// Configures the AI's eye for us
    /// </summary>
    /// <param name="shellBrain"></param>
    /// <param name="mind">Mind that we want to transfer. Use if </param>
    /// <param name="core"></param>
    public void ExitShell(Entity<StationAiShellBrainComponent?> shellBrain, Entity<MindComponent?> mind = default)
    {
        if (!Resolve(shellBrain, ref shellBrain.Comp))
            return;

        // Try to get the mind of the brain if no other minds were passed in
        // Useful for when the mind isn't inside the shell
        if (mind == default && !_mind.TryGetMind(shellBrain, out mind.Owner, out mind.Comp) ||
            !Resolve(mind, ref mind.Comp))
            return;

        // Ensure that we have a core with a mind to go to
        if (!shellBrain.Comp.ActiveCore.HasValue)
            return;

        if(!_stationAiSystem.TryGetCore(shellBrain.Comp.ActiveCore.Value, out var core) ||
           core.Comp == null)
            return;

        _stationAiSystem.SwitchRemoteEntityMode(core, true);
        _mind.TransferTo(mind, shellBrain.Comp.ActiveCore.Value, mind: mind.Comp);
        if (core.Comp.RemoteEntity.HasValue)
            _xforms.SetCoordinates(core.Comp.RemoteEntity.Value, Transform(shellBrain).Coordinates);

        shellBrain.Comp.ActiveCore = null;
    }

    private void OnIonStormLaws(Entity<StationAiShellUserComponent> ent, ref IonStormLawsEvent args)
    {
        ChangeShellLaws(ent, args.Lawset);
    }

    /// <summary>
    /// Changes the laws of the given shell user's shell
    /// Will only change the laws of the currently selected shell, not all shells it has listed
    /// </summary>
    /// <param name="entity">The entity of the shell user</param>
    /// <param name="lawset">The lawset we want to change to</param>
    /// <param name="cue">A provided sound cue to play when we want to change the laws. Tries to get a sound from the shell user entity if not provided</param>
    public virtual void ChangeShellLaws(EntityUid entity, SiliconLawset? lawset, SoundSpecifier? cue = null)
    {
    }

    protected virtual void AddChannels(Entity<BorgChassisComponent?> chassis, Entity<StationAiShellUserComponent> shellUser)
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
