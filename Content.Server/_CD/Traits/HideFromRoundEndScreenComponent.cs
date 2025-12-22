using Content.Shared.Traits;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Traits;

[RegisterComponent]
public sealed partial class HideFromRoundEndScreenComponent : Component
{
    public static readonly ProtoId<TraitPrototype> TraitName = "HideFromRoundEndScreen";
}
