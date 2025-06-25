using Content.Server.Radio.Components;
using Content.Shared._CD.Silicons.StationAi;
using Content.Shared.Radio;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Server._CD.Silicons.StationAi;

public sealed class StationAiShellUserSystem : SharedStationAiShellUserSystem
{
    /// <summary>
    /// Adds the AI's existing radio channels to the chassis upon taking control
    /// </summary>
    public override void AddChannels(Entity<BorgChassisComponent?> chassis, Entity<StationAiShellUserComponent?> shellUser)
    {
        if (shellUser.Comp == null)
            return;

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
    public override void RemoveChannels(Entity<BorgChassisComponent?> chassis, Entity<StationAiHeldComponent?> held)
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
}
