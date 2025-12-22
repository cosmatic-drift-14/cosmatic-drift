using Content.Server.Silicons.Laws;
using Content.Shared._CD.Silicons.StationAi;
using Content.Shared.Radio.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.Laws;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Audio;

namespace Content.Server._CD.Silicons.StationAi;

public sealed class StationAiShellUserSystem : SharedStationAiShellUserSystem
{
    [Dependency] private readonly SiliconLawSystem _laws = default!;
    [Dependency] private readonly StationAiShellBrainSystem _shellBrain = default!;

    protected override void OnEnterShell(Entity<StationAiShellUserComponent> ent, ref EnterShellMessage args)
    {
        base.OnEnterShell(ent, ref args);

        if (ent.Comp.SelectedBrain != null)
            _shellBrain.SetShellName(ent.Comp.SelectedBrain.Value);
    }

    /// <summary>
    /// Adds the AI's existing radio channels to the chassis upon taking control
    /// </summary>
    protected override void AddChannels(Entity<BorgChassisComponent?> chassis, Entity<StationAiShellUserComponent> shellUser)
    {
        if (!TryComp(shellUser, out ActiveRadioComponent? shellUserRadio))
            return;

        var activeRadio = EnsureComp<ActiveRadioComponent>(chassis);

        foreach (var channel in shellUserRadio.Channels)
        {
            if (activeRadio.Channels.Add(channel))
                shellUser.Comp.ActiveAddedChannels.Add(channel);
        }

        EnsureComp<IntrinsicRadioReceiverComponent>(chassis);

        var intrinsicRadioTransmitter = EnsureComp<IntrinsicRadioTransmitterComponent>(chassis);
        foreach (var channel in shellUserRadio.Channels)
        {
            if (intrinsicRadioTransmitter.Channels.Add(channel))
            {
                shellUser.Comp.TransmitterAddedChannels.Add(channel);
            }
        }
    }

    /// <summary>
    /// Removes all added channels from the chassis upon exiting and returning to the normal AI state
    /// </summary>
    protected override void RemoveChannels(Entity<BorgChassisComponent?> chassis, Entity<StationAiHeldComponent?> held)
    {
        if (!TryComp<StationAiShellUserComponent>(held, out var shellUser))
            return;

        if (!TryComp<ActiveRadioComponent>(chassis, out var activeRadio))
            return;

        foreach (var channel in shellUser.ActiveAddedChannels)
        {
            activeRadio.Channels.Remove(channel);
        }
        shellUser.ActiveAddedChannels.Clear();

        if (!TryComp<IntrinsicRadioTransmitterComponent>(chassis, out var intrinsicRadioTransmitter))
            return;

        foreach (var channel in shellUser.TransmitterAddedChannels)
        {
            intrinsicRadioTransmitter.Channels.Remove(channel);
        }
        shellUser.TransmitterAddedChannels.Clear();
    }

    /// <summary>
    /// Add a shell to an AI's list of controllable shells
    ///  </summary>
    /// <param name="shellUser">The AI that is capable of controlling shells</param>
    /// <param name="shell">The shell to be made available to the controlling AI</param>
    public void AddToAvailableShells(Entity<StationAiShellUserComponent> shellUser, Entity<BorgChassisComponent> shell)
    {
        shellUser.Comp.ControllableShells.Add(shell);
        Dirty<StationAiShellUserComponent?>(shellUser.Owner);
    }

    /// <summary>
    /// Remove a shell from an AI's list of controllable shells
    /// </summary>
    /// <param name="shellUser">The AI that is capable of controlling shells</param>
    /// <param name="shell">The shell to be made available to the controlling AI</param>
    public void RemoveFromAvailableShells(Entity<StationAiShellUserComponent> shellUser, Entity<BorgChassisComponent> shell)
    {
        shellUser.Comp.ControllableShells.Remove(shell);
        Dirty<StationAiShellUserComponent?>(shellUser.Owner);

    }

    /// <inheritdoc />
    public override void ChangeShellLaws(EntityUid entity, SiliconLawset? lawset, SoundSpecifier? cue = null)
    {
        base.ChangeShellLaws(entity, lawset, cue);

        // Checks
        if (!TryComp<StationAiShellUserComponent>(entity, out var shellUser))
            return;

        if (shellUser.SelectedShell == null)
            return;

        if (lawset == null)
            return;

        // Fallback for when a sound cue isn't provided but the entity has a lawprovider component
        if (cue == null && TryComp<SiliconLawProviderComponent>(entity, out var comp))
        {
            cue = comp.LawUploadSound;
        }

        _laws.SetLaws(lawset.Laws, shellUser.SelectedShell.Value, cue);
    }
}
