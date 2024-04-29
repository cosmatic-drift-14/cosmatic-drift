using Content.Server.GameTicking;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Preferences;
using Content.Shared.Traits;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Traits;

public sealed class TraitSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;
    [Dependency] private readonly SharedHandsSystem _sharedHandsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    /// <summary>
    /// Attempts to add traits to the entity.
    /// </summary>
    /// <returns></returns>
    public bool TryAddTraits(EntityUid mob, HumanoidCharacterProfile characterProfile)
    {
        foreach (var traitId in characterProfile.TraitPreferences)
        {
            if (!_prototypeManager.TryIndex<TraitPrototype>(traitId, out var traitPrototype))
            {
                Log.Warning($"No trait found with ID {traitId}!");
                return false;
            }

            if (traitPrototype.Whitelist != null && !traitPrototype.Whitelist.IsValid(mob))
                continue;

            if (traitPrototype.Blacklist != null && traitPrototype.Blacklist.IsValid(mob))
                continue;

            // Add all components required by the prototype
            foreach (var entry in traitPrototype.Components.Values)
            {
                if (HasComp(mob, entry.Component.GetType()))
                    continue;

                var comp = (Component) _serializationManager.CreateCopy(entry.Component, notNullableOverride: true);
                comp.Owner = mob;
                EntityManager.AddComponent(mob, comp);
            }

            // Add item required by the trait
            if (traitPrototype.TraitGear != null)
            {
                if (!TryComp(mob, out HandsComponent? handsComponent))
                    continue;

                var coords = Transform(mob).Coordinates;
                var inhandEntity = EntityManager.SpawnEntity(traitPrototype.TraitGear, coords);
                _sharedHandsSystem.TryPickup(mob, inhandEntity, checkActionBlocker: false,
                    handsComp: handsComponent);
            }
        }

        return false;
    }

    // When the player is spawned in, add all trait components selected during character creation
    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        TryAddTraits(args.Mob, args.Profile);
    }
}
