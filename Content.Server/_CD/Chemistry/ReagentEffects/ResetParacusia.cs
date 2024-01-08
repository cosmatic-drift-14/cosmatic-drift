using Content.Server.Traits.Assorted;
using Content.Shared.Chemistry.Reagent;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Chemistry.ReagentEffects;


[UsedImplicitly]
public sealed partial class ResetParacusia : ReagentEffect
{
    [DataField("TimerReset")]
    public int TimerReset = 600;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-reset-paracusia", ("chance", Probability));

    public override void Effect(ReagentEffectArgs args)
    {
        if (args.Scale != 1f)
            return;

        var sys = args.EntityManager.EntitySysManager.GetEntitySystem<ParacusiaSystem>();
        sys.SetIncidentDelay(args.SolutionEntity, new TimeSpan(0, 0, TimerReset));
    }
}
