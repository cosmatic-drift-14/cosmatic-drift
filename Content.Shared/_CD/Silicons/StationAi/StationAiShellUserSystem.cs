using System.Runtime.InteropServices;
using Content.Shared.Actions;
using Content.Shared.Mind;
using Content.Shared.Movement.Systems;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Microsoft.VisualBasic.CompilerServices;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Shared._CD.Silicons.StationAi;

public sealed class StationAiShellUserSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedStationAiSystem _stationAiSystem = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAiShellUserComponent, AiEnterShellEvent>(OnEnterShell);
        SubscribeLocalEvent<BorgChassisComponent, AiExitShellEvent>(OnExitShell);
    }

    private void OnOpenUi(Entity<StationAiShellUserComponent> ent, ref AiEnterShellEvent args)
    {

    }

    private void OnEnterShell(Entity<StationAiShellUserComponent> ent, ref AiEnterShellEvent args)
    {
        if (!TryComp<StationAiHeldComponent>(ent.Owner, out var held)) // Check that the user is an AI
            return;

        if (!_stationAiSystem.TryGetStationAiCore((ent.Owner, held), out var core)) // And check that they have a core
            return;

        // Now, select a shell
        if (_net.IsClient)
            return;

        // TODO: This should open a list, let the AI select one then either teleport to it or possess it.
        // This query would be the list logic
        var query = EntityQueryEnumerator<BorgChassisComponent>();
        while (query.MoveNext(out var uid, out var chassis))
        {
            if (!chassis.BrainEntity.HasValue && !HasComp<StationAiShellBrainComponent>(chassis.BrainEntity)) // First make sure the selected chassis has a brain, and it's a BORIS module
                return;

            ent.Comp.SelectedShell = uid;
            ent.Comp.SelectedBrain = chassis.BrainEntity;
        }

        // Anything below this would be the "on possess" button being pressed
        if (!_mind.TryGetMind(ent.Owner, out var mindId, out var mind) || mind.Session == null) // Then get the AI's mind
            return;

        if (!ent.Comp.SelectedBrain.HasValue || !ent.Comp.SelectedShell.HasValue ||
            !TryComp<StationAiShellBrainComponent>(ent.Comp.SelectedBrain.Value, out var shellBrain)) // Get the brain of the shell
            return;

        shellBrain.ActiveCore = ent.Owner;
        _mind.TransferTo(mindId, ent.Comp.SelectedShell, mind: mind);
        _actions.AddAction(ent.Comp.SelectedShell.Value, ref ent.Comp.ActionEntity, ent.Comp.ActionPrototype);

        // Put the eye at the core
        if(core.Value.Comp.RemoteEntity.HasValue)
            _xforms.DropNextTo(core.Value.Comp.RemoteEntity.Value, core.Value.Owner);

        // Set the chassis' name to the AI's
        var metaData = MetaData(ent.Owner);
        _metaData.SetEntityName(ent.Comp.SelectedShell.Value, metaData.EntityName);
    }

    private void OnExitShell(Entity<BorgChassisComponent> ent, ref AiExitShellEvent args)
    {
        if (!_mind.TryGetMind(ent.Owner, out var mindId, out var mind) || mind.Session == null) // First get our brain
            return;

        if (!TryComp<StationAiShellBrainComponent>(ent.Comp.BrainEntity, out var shellBrain))
            return;

        var brainUid = shellBrain.ActiveCore;
        if (!brainUid.HasValue || !TryComp<StationAiHeldComponent>(brainUid, out var held)) // Check that the user is an AI
            return;

        if (!_stationAiSystem.TryGetStationAiCore((brainUid.Value, held), out var core)) // And check that they have a core
            return;

        _mind.TransferTo(mindId, brainUid, mind: mind);
        _stationAiSystem.SetupEye(core.Value);
        _stationAiSystem.AttachEye(core.Value);

        if(core.Value.Comp.RemoteEntity.HasValue)
            _xforms.DropNextTo(core.Value.Comp.RemoteEntity.Value, ent.Owner);
    }

}

public sealed partial class AiEnterShellEvent : InstantActionEvent
{
}

public sealed partial class AiExitShellEvent : InstantActionEvent
{
}



