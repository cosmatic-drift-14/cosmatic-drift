using Content.Shared.DoAfter;
using Content.Shared._CD.TapeRecorder.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.TapeRecorder.Events;

[Serializable, NetSerializable]
public sealed partial class TapeCassetteRepairDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class ChangeModeTapeRecorderMessage : BoundUserInterfaceMessage
{
    public TapeRecorderMode Mode;

    public ChangeModeTapeRecorderMessage(TapeRecorderMode mode)
    {
        Mode = mode;
    }
}

[Serializable, NetSerializable]
public sealed class PrintTapeRecorderMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class TapeRecorderState : BoundUserInterfaceState
{
    // TODO: check the itemslot on client instead of putting easy cassette stuff in the state
    public bool HasCassette;
    public bool HasData;
    public float CurrentTime;
    public float MaxTime;
    public string CassetteName;
    public TimeSpan PrintCooldown;

    public TapeRecorderState(
        bool hasCassette,
        bool hasData,
        float currentTime,
        float maxTime,
        string cassetteName,
        TimeSpan printCooldown)
    {
        HasCassette = hasCassette;
        HasData = hasData;
        CurrentTime = currentTime;
        MaxTime = maxTime;
        CassetteName = cassetteName;
        PrintCooldown = printCooldown;
    }
}
