using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Shared._CD.Clothing;

public sealed class ClothingBlockSlotsOnEquipSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingBlockSlotsOnEquipComponent, BeingEquippedAttemptEvent>(OnBeingEquippedAttempt);
        SubscribeLocalEvent<ClothingBlockSlotsOnEquipComponent, IsEquippingAttemptEvent>(OnIsEquippingAttempt);
        SubscribeLocalEvent<InventoryComponent, IsEquippingAttemptEvent>(OnEquippingAttempt);
    }

    private void OnBeingEquippedAttempt(Entity<ClothingBlockSlotsOnEquipComponent> ent, ref BeingEquippedAttemptEvent args)
    {
        if (!HasComp<InventoryComponent>(args.EquipTarget))
            return;

        foreach (var slot in ent.Comp.BlockedSlots)
        {
            if (!_inventory.TryGetSlotEntity(args.EquipTarget, slot, out var existingItem))
                continue;

            var name = MetaData(existingItem.Value).EntityName;
            args.Reason = Loc.GetString("clothing-blocked-slots-blocked", ("item", name));
            args.Cancel();
            return;
        }
    }

    private void OnIsEquippingAttempt(Entity<ClothingBlockSlotsOnEquipComponent> ent, ref IsEquippingAttemptEvent args)
    {
        if (!ent.Comp.BlockedSlots.Contains(args.Slot))
            return;

        var name = MetaData(ent.Owner).EntityName;
        args.Reason = Loc.GetString("clothing-blocked-slots-blocked", ("item", name));
        args.Cancel();
    }

    private void OnEquippingAttempt(Entity<InventoryComponent> ent, ref IsEquippingAttemptEvent args)
    {
        var enumerator = _inventory.GetSlotEnumerator(ent.Owner);
        while (enumerator.NextItem(out var equipped, out _))
        {
            if (!TryComp<ClothingBlockSlotsOnEquipComponent>(equipped, out var blockComp))
                continue;

            if (!blockComp.BlockedSlots.Contains(args.Slot))
                continue;

            var name = MetaData(equipped).EntityName;
            args.Reason = Loc.GetString("clothing-blocked-slots-blocked", ("item", name));
            args.Cancel();
            return;
        }
    }
}
