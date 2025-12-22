using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Utility;

namespace Content.Shared._CD.Clothing;

public sealed class ClothingBlockSlotsOnEquipSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClothingBlockSlotsOnEquipComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<ClothingBlockSlotsOnEquipComponent, BeingEquippedAttemptEvent>(OnBeingEquippedAttempt);
        SubscribeLocalEvent<ClothingBlockSlotsOnEquipComponent, IsEquippingAttemptEvent>(OnIsEquippingAttempt);
        SubscribeLocalEvent<InventoryComponent, IsEquippingAttemptEvent>(OnEquippingAttempt);
    }

    /// <summary>
    /// Sets the description to indicate what slots will be blocked by wearing this item.
    /// </summary>
    private void OnExamined(EntityUid uid, ClothingBlockSlotsOnEquipComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        // we push it as a message so that we can add a newline at the end
        var msg = new FormattedMessage();
        msg.AddText(Loc.GetString("clothing-blocked-slots-description"));

        foreach (var slot in component.BlockedSlots)
        {
            msg.PushNewline();
            var slotName = Loc.GetString($"clothing-slot-{slot}");
            msg.AddMarkupOrThrow(Loc.GetString("clothing-blocked-slots-entry", ("slot", slotName)));
        }

        msg.PushNewline();
        args.PushMessage(msg, 1);
    }

    /// <summary>
    /// Checks if we can equip the item.
    /// </summary>
    /// <remarks>This is called to see if the specified slots are empty.</remarks>
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

    /// <summary>
    /// Checks if we can equip the item.
    /// </summary>
    /// <remarks>This is called to see if we're not trying to equip it in a slot it would block.</remarks>
    private void OnIsEquippingAttempt(Entity<ClothingBlockSlotsOnEquipComponent> ent, ref IsEquippingAttemptEvent args)
    {
        if (!ent.Comp.BlockedSlots.Contains(args.Slot))
            return;

        var name = MetaData(ent.Owner).EntityName;
        args.Reason = Loc.GetString("clothing-blocked-slots-blocked", ("item", name));
        args.Cancel();
    }

    /// <summary>
    /// Checks if we can equip other items.
    /// </summary>
    /// <remarks>This is called whenever we try to equip an item to see if the slot isn't currently blocked.</remarks>
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
