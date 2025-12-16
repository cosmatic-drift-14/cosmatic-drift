using Content.Shared._CD.Silicons;
using Content.Shared._CD.Silicons.Borgs;
using Content.Shared.Inventory;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Silicons.Borgs;

/// <summary>
/// Server-side logic that shouldn't be exposed to the client.
/// </summary>
public sealed class BorgSwitchableSubstypeSystem : SharedBorgSwitchableSubtypeSystem
{
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    protected override void SelectBorgSubtype(Entity<BorgSwitchableSubtypeComponent> ent)
    {
        if (ent.Comp.BorgSubtype == null)
            return;

        var borgSubtype = Prototypes.Index(ent.Comp.BorgSubtype.Value);

        // Configure special components
        if (Prototypes.TryIndex(ent.Comp.BorgSubtype, out var previousPrototype))
        {
            if (previousPrototype.AddComponents is { } removeComponents)
                EntityManager.RemoveComponents(ent, removeComponents);
        }

        if (borgSubtype.AddComponents is { } addComponents)
        {
            EntityManager.AddComponents(ent, addComponents);
        }

        // inventory template configuration (hats spacing)
        if (TryComp(ent, out InventoryComponent? inventory))
        {
            _inventorySystem.SetTemplateId((ent.Owner, inventory), borgSubtype.InventoryTemplateId);
        }

        Dirty(ent);
        base.SelectBorgSubtype(ent);
    }
}
