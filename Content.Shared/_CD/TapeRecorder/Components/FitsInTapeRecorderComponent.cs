using Robust.Shared.GameStates;

/// <summary>
/// Removed from the cassette when damaged to prevent it being played until repaired
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FitsInTapeRecorderComponent : Component;
