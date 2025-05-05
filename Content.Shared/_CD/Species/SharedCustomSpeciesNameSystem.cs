using Content.Shared.Humanoid;

namespace Content.Shared._CD.Species;

public abstract class SharedCustomSpeciesNameSystem : EntitySystem
{
    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    public string GetSpeciesName(Entity<HumanoidAppearanceComponent> ent)
    {
        if (TryComp<CustomSpeciesNameComponent>(ent, out var customSpecies))
        {
            return customSpecies.NewSpeciesName;
        }

        return _humanoidAppearance.GetSpeciesRepresentation(ent.Comp.Species);
    }
}
