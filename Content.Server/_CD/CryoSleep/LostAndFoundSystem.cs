using Content.Server.Hands.Systems;
using Content.Shared._CD.CryoSleep;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using System.Linq;

namespace Content.Server._CD.CryoSleep;

public sealed class LostAndFoundSystem : EntitySystem
{
    [Dependency] private readonly HandsSystem _hands = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    private const string LostFoundContainer = "lost_found_storage";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LostAndFoundComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<LostAndFoundComponent, BoundUIOpenedEvent>(OnUIOpened);
        SubscribeLocalEvent<LostAndFoundComponent, LostAndFoundRetrieveItemMessage>(OnRetrieveItem);
    }

    private void OnMapInit(Entity<LostAndFoundComponent> ent, ref MapInitEvent args)
    {
        _container.EnsureContainer<Container>(ent, LostFoundContainer);
    }

    private void OnUIOpened(Entity<LostAndFoundComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUI(ent);
    }

    private void UpdateUI(Entity<LostAndFoundComponent> ent)
    {
        var state = new LostAndFoundBuiState(ent.Comp.StoredItems);
        _ui.SetUiState(ent.Owner, LostAndFoundUiKey.Key, state);
    }

    /// <summary>
    /// Called when a player's items are being stored in the lost and found.
    /// </summary>
    public void StorePlayerItems(Entity<LostAndFoundComponent> ent, string playerName, List<LostItemData> items)
    {
        var container = _container.EnsureContainer<Container>(ent, LostFoundContainer);

        // Store all items in the container
        foreach (var item in items.Select(itemData => GetEntity(itemData.ItemEntity)).Where(Exists))
        {
            // Move item to storage container
            _container.Insert(item, container);
        }

        ent.Comp.StoredItems[playerName] = items;
        Dirty(ent);
        UpdateUI(ent);
    }

    private void OnRetrieveItem(EntityUid uid, LostAndFoundComponent component, LostAndFoundRetrieveItemMessage args)
    {
        var ent = (uid, component);

        // Validate that the player exists in storage
        if (!ent.component.StoredItems.TryGetValue(args.PlayerName, out var items))
            return;

        // Find the specific item
        var itemData = items.FirstOrDefault(i => i.SlotName == args.SlotName);
        if (itemData == null)
            return;

        // Try to get the actual entity from storage
        var itemUid = GetEntity(itemData.ItemEntity);
        if (!Exists(itemUid))
            return;

        var userUid = GetEntity(args.User);
        if (!TryRetrieveItem(uid, itemUid, userUid))
            return;

        // Remove item from records only if successfully retrieved
        items.Remove(itemData);
        if (items.Count == 0)
            ent.component.StoredItems.Remove(args.PlayerName);

        Dirty(uid, component);
        UpdateUI(ent);
    }


    private bool TryRetrieveItem(EntityUid storageUid, EntityUid itemUid, EntityUid userUid)
    {
        // Ensure storage container exists
        var container = _container.EnsureContainer<Container>(storageUid, LostFoundContainer);
        if (!container.Contains(itemUid))
            return false;

        // Calculate the destination coordinates (in front of the user)
        var userXform = Transform(userUid);
        var coordinates = userXform.Coordinates;

        // Remove from container
        if (!_container.Remove(itemUid, container, force: true))
            return false;

        // Set item position and try to pick it up
        _transform.SetCoordinates(itemUid, coordinates);
        _hands.TryPickup(userUid, itemUid);

        return true;
    }
}
