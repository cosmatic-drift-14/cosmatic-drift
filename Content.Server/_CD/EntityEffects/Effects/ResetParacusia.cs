using Content.Server.Traits.Assorted;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.EntityEffects.Effects;


[UsedImplicitly]
public sealed partial class ResetParacusia : EntityEffect
{
    [DataField("TimerReset")]
    public int TimerReset = 600;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-reset-paracusia", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is EntityEffectReagentArgs reagentArgs)
        {
            if (reagentArgs.Scale.Float() != 1f)
                return;
        }

        var sys = args.EntityManager.EntitySysManager.GetEntitySystem<ParacusiaSystem>();
        sys.SetIncidentDelay(args.TargetEntity, new TimeSpan(0, 0, TimerReset));
    }
}
