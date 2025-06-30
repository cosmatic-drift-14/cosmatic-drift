using Content.Shared.Traits.Assorted;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Shared._CD.EntityEffects.Effects;


[UsedImplicitly]
public sealed partial class ResetParacusia : EventEntityEffect<ResetParacusia>
{
    [DataField("TimerReset")]
    public int TimerReset = 600;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-reset-paracusia", ("chance", Probability));
}
