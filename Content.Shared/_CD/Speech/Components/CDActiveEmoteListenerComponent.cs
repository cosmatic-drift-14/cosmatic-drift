using Content.Shared.Chat;
using Content.Shared.Speech.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._CD.Speech.Components;

/// <summary>
/// Emote version of <see cref="ActiveListenerComponent"/>
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class CDActiveEmoteListenerComponent : Component
{
    /// <summary>
    /// The range in which to listen to speech.
    /// </summary>
    [DataField]
    public float Range = SharedChatSystem.VoiceRange;
}
