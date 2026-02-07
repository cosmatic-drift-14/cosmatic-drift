using System.Linq;
using Content.Shared._CD.TapeRecorder;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Popups;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._CD;

// TODO CD: docs, sanitize, and saneify

/// <summary>
/// This handles...
/// </summary>

public sealed class ExtendableClothingSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ExtendableClothingComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ExtendableClothingComponent, BeingEquippedAttemptEvent>(OnAttemptEquip, after: new[] {typeof(SharedVirtualItemSystem)});
        SubscribeLocalEvent<ExtendableClothingComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ExtendableClothingComponent, GotUnequippedEvent>(OnAttemptUnequip);

        SubscribeLocalEvent<ExtendedEquipmentComponent, BeingUnequippedAttemptEvent>(OnSubequipmentAttemptUnequip);
    }

    private void OnEquipped(Entity<ExtendableClothingComponent> ent, ref GotEquippedEvent args)
    {
        var container = _container.GetContainer(ent, ExtendableClothingComponent.EquipmentContainerId);

        foreach (var equipment in container.ContainedEntities.ToList())
        {
            if (!TryComp<ExtendedEquipmentComponent>(equipment, out var comp))
                return;

            if(!_inventory.TryEquip(args.Equipee, args.Equipee, equipment, comp.Slot, force: true))
                return;

            ent.Comp.CurrentlyEquipped.Add(equipment);
        }

        Dirty(ent);
    }

    private void OnSubequipmentAttemptUnequip(Entity<ExtendedEquipmentComponent> ent, ref BeingUnequippedAttemptEvent args)
    {
        // TODO localize
        args.Reason = $"Can't unequip, item is attached to {Name(ent.Comp.ParentEquipment)}!";
        args.Cancel();
    }

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

    private void OnAttemptEquip(Entity<ExtendableClothingComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        var container = _container.GetContainer(ent, ExtendableClothingComponent.EquipmentContainerId);

        foreach (var equipment in container.ContainedEntities)
        {
            if (!TryComp<ExtendedEquipmentComponent>(equipment, out var comp))
                return;

            if (_inventory.TryGetSlotEntity(args.EquipTarget, comp.Slot, out var slotDef))
            {
                args.Cancel();
                return;
            }
        }
    }

    private void OnAttemptUnequip(Entity<ExtendableClothingComponent> ent, ref GotUnequippedEvent args)
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
}
