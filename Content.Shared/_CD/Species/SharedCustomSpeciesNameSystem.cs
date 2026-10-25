using Content.Shared.Humanoid;

namespace Content.Shared._CD.Species;

public abstract partial class SharedCustomSpeciesNameSystem : EntitySystem
{
    [Dependency] private HumanoidProfileSystem _humanoidProfile = default!;
    public string GetSpeciesName(Entity<HumanoidProfileComponent> ent)
    {
        if (TryComp<CustomSpeciesNameComponent>(ent, out var customSpecies))
        {
            return customSpecies.NewSpeciesName;
        }

        return _humanoidProfile.GetSpeciesRepresentation(ent.Comp.Species);
    }
}
