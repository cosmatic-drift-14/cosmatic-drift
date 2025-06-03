using System.Linq;
using Content.Server.Body.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Robust.Shared.Timing;

namespace Content.Server.Body.Systems;

public sealed class AllergySystem : EntitySystem
{
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);

        SubscribeLocalEvent<AllergyComponent, ComponentInit>(OnAllergyInit);

        SubscribeLocalEvent<AllergyComponent, EntityUnpausedEvent>(OnUnpaused);
        SubscribeLocalEvent<AllergyComponent, ApplyMetabolicMultiplierEvent>(OnApplyMetabolicMultiplier);

        SubscribeLocalEvent<AllergyComponent, ReactionEntityEvent>(OnReaction);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        var allergies = args.Profile.CDCharacterRecords?.Allergies;
        if (allergies is { Count: > 0 })
            AddComp(args.Mob, new AllergyComponent { Reagents = allergies });
    }

    private void OnAllergyInit(EntityUid uid, AllergyComponent component, ComponentInit args)
    {
        component.NextUpdate = _gameTiming.CurTime + component.UpdateInterval;
    }

    private void OnUnpaused(EntityUid uid, AllergyComponent component, ref EntityUnpausedEvent args)
    {
        component.NextUpdate += args.PausedTime;
    }

    // straight up copypasted from MetabolizerSystem
    private void OnApplyMetabolicMultiplier(
        Entity<AllergyComponent> ent,
        ref ApplyMetabolicMultiplierEvent args)
    {
        // TODO REFACTOR THIS
        // This will slowly drift over time due to floating point errors.
        // Instead, raise an event with the base rates and allow modifiers to get applied to it.
        if (args.Apply)
        {
            ent.Comp.UpdateInterval *= args.Multiplier;
            return;
        }

        ent.Comp.UpdateInterval /= args.Multiplier;
    }

    private void OnReaction(EntityUid uid, AllergyComponent allergy, ref ReactionEntityEvent args)
    {
        if (!allergy.Reagents.TryGetValue(args.Reagent.ID, out var reaction))
            return;
        if (!TryComp(uid, out BloodstreamComponent? bloodstream))
            return;
        if (!_solutionContainerSystem.ResolveSolution(uid,
                bloodstream.ChemicalSolutionName,
                ref bloodstream.ChemicalSolution,
                out var solution))
            return;
        var quantity = args.ReagentQuantity.Quantity;
        solution.AddReagent(new ReagentId("Histamine", null), reaction * quantity);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<AllergyComponent, BloodstreamComponent>();
        while (query.MoveNext(out var uid, out var allergy, out var bloodstream))
        {
            if (_gameTiming.CurTime < allergy.NextUpdate)
                continue;

            allergy.NextUpdate += allergy.UpdateInterval;

            if (allergy.Reagents.Count == 0)
                continue;

            if (!_solutionContainerSystem.ResolveSolution(uid,
                    bloodstream.ChemicalSolutionName,
                    ref bloodstream.ChemicalSolution,
                    out var chemstream))
                continue;

            chemstream.AddReagent(
                new ReagentId("Histamine", null),
                Enumerable.SelectMany([
                            chemstream,
                            .._bodySystem.GetBodyOrganEntityComps<LungComponent>(uid)
                                .Where(lung => lung.Comp1.Solution is not null)
                                .Select(lung => lung.Comp1.Solution!.Value.Comp.Solution),
                        ],
                        solution => solution.Contents)
                    .Aggregate(new FixedPoint2(),
                        (reaction, reagent) =>
                            reaction + allergy.Reagents.GetValueOrDefault(reagent.Reagent.Prototype) *
                            reagent.Quantity));
        }
    }
}
