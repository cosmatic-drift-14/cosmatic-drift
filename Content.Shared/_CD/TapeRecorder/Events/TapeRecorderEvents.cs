using Content.Shared.DoAfter;
using Content.Shared._CD.TapeRecorder.Components;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.TapeRecorder.Events;

[Serializable, NetSerializable]
public sealed partial class TapeCassetteRepairDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed class ChangeModeTapeRecorderMessage(TapeRecorderMode mode) : BoundUserInterfaceMessage
{
    public TapeRecorderMode Mode = mode;
}

[Serializable, NetSerializable]
public sealed class PrintTapeRecorderMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class TapeRecorderState(
    bool hasCassette,
    float currentTime,
    float maxTime,
    string cassetteName,
    TimeSpan printCooldown): BoundUserInterfaceState
{
    // TODO: check the itemslot on client instead of putting easy cassette stuff in the state
    public bool HasCassette = hasCassette;
    public float CurrentTime = currentTime;
    public float MaxTime = maxTime;
    public string CassetteName = cassetteName;
    public TimeSpan PrintCooldown = printCooldown;
}
