using System.Linq;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Inventory.VirtualItem;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CD;

public sealed class ExtendableClothingSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ExtendedEquipmentComponent, BeingUnequippedAttemptEvent>(OnSubequipmentAttemptUnequip);

        SubscribeLocalEvent<ExtendableClothingComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ExtendableClothingComponent, BeingEquippedAttemptEvent>(OnAttemptEquip, after: new[] {typeof(SharedVirtualItemSystem)});
        SubscribeLocalEvent<ExtendableClothingComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ExtendableClothingComponent, GotUnequippedEvent>(OnUnequipped);

        SubscribeLocalEvent<ExtendableClothingComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<ExtendableClothingComponent> ent, ref ExaminedEvent args)
    {
        using (args.PushGroup(nameof(ExtendableClothingComponent)))
        {
            args.PushMarkup(Loc.GetString("cd-extendable-clothing-description"));

            foreach (var inventorySlot in ent.Comp.AttachedEquipment.Keys)
            {
                var slotName = Loc.GetString($"clothing-slot-{inventorySlot}");
                args.PushMarkup(Loc.GetString("cd-extendable-clothing-entry", ("slot", slotName)));
            }
        }
    }

    /// <summary>
    /// Checks if the slots we want to equip equipment to are empty. If not, cancel the attempt.
    /// </summary>
    private void OnAttemptEquip(Entity<ExtendableClothingComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var container = _container.GetContainer(ent, ExtendableClothingComponent.EquipmentContainerId);
        foreach (var equipment in container.ContainedEntities)
        {
            DebugTools.Assert(!equipment.IsValid());
            if (!TryComp<ExtendedEquipmentComponent>(equipment, out var comp))
                return;

            if (_inventory.TryGetSlotEntity(args.EquipTarget, comp.Slot, out var slotEnt))
            {
                args.Cancel();
                return;
            }
        }
    }

    /// <summary>
    /// Logic for equipping all the equipment that is 'attached' to the parent clothing.
    /// </summary>
    private void OnEquipped(Entity<ExtendableClothingComponent> ent, ref GotEquippedEvent args)
    {
        var container = _container.GetContainer(ent, ExtendableClothingComponent.EquipmentContainerId);

        foreach (var equipment in container.ContainedEntities.ToList())
        {
            // already made sure the entities have the proper component in the attempt
            var comp = Comp<ExtendedEquipmentComponent>(equipment);
            if(!_inventory.TryEquip(args.Equipee, args.Equipee, equipment, comp.Slot, force: true))
                return;

            ent.Comp.CurrentlyEquipped.Add(equipment);
        }

        Dirty(ent);
    }

    /// <summary>
    /// Unequips all the attached equipment, making sure they all go back into the parent clothing's container.
    /// </summary
    private void OnUnequipped(Entity<ExtendableClothingComponent> ent, ref GotUnequippedEvent args)
    {
        if (!_container.TryGetContainer(ent, ExtendableClothingComponent.EquipmentContainerId, out var container))
            return;

        foreach (var entityUid in ent.Comp.CurrentlyEquipped.ToList())
        {
            _container.Insert(entityUid, container, force: true);
            ent.Comp.CurrentlyEquipped.Remove(entityUid);
        }

        Dirty(ent);
    }

    private void OnSubequipmentAttemptUnequip(Entity<ExtendedEquipmentComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        // we don't want to allow attached equipment to be removed directly,
        // ensure that said equipment is only unequippable by the parent
        args.Reason = Loc.GetString("cd-extended-clothing-blocked",
            ("extended", Name(args.Equipment)),
            ("parent", Name(ent.Comp.ParentEquipment)));
        args.Cancel();
    }

    /// <summary>
    /// Spawns all our attached equipment inside the parent, then ensures they're initialized properly.
    /// </summary>
    private void OnMapInit(Entity<ExtendableClothingComponent> ent, ref MapInitEvent args)
    {
        _container.EnsureContainer<Container>(ent, ExtendableClothingComponent.EquipmentContainerId);

        foreach (var proto in ent.Comp.AttachedEquipment)
        {
            if (!_proto.TryIndex(proto.Value, out var entProto))
                return;

            if(!TrySpawnInContainer(entProto.ID, ent, ExtendableClothingComponent.EquipmentContainerId, out var equipmentUid))
                return;

            EnsureComp<ExtendedEquipmentComponent>(equipmentUid.Value, out var comp);
            comp.Slot = proto.Key;
            comp.ParentEquipment = ent;
            Dirty(equipmentUid.Value, comp);
        }
    }
}
