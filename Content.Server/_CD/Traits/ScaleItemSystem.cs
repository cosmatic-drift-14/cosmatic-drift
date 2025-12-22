// using System.Threading;
// using Content.Server.Resist;
// using Content.Shared.Humanoid;
// using Content.Shared.Item;
//
// namespace Content.Server._CD.Traits;
// /// <summary>
// /// Itemizes the player, with optional checks and alerting the player with a UI.
// /// </summary>
// public sealed class ScaleItemSystem : EntitySystem
// {
//     [Dependency] private readonly SharedItemSystem _itemSystem = default!;
//
//     public override void Initialize()
//     {
//         base.Initialize();
//
//         SubscribeLocalEvent<ScaleItemComponent, ComponentInit>(OnComponentInit);
//     }
//
//     private void OnComponentInit(EntityUid uid, ScaleItemComponent comp, ComponentInit args)
//     {
//         // Ensure they have a humanoidappearancecomponent, and thus, a scale
//         if (!TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
//             return;
//
//         // Then check if the minscale / maxscale exist and have values
//         if (comp.MinScale.HasValue && comp.MinScale > humanoid.Height)
//             return;
//
//         if (comp.MaxScale.HasValue && comp.MaxScale < humanoid.Height)
//             return;
//
//         // Finally, actually itemize them.
//         ItemizePlayer(uid, comp);
//     }
//
//     private void ItemizePlayer(EntityUid uid, ScaleItemComponent comp)
//     {
//         var itemComp = EnsureComp<ItemComponent>(uid);
//         _itemSystem.SetSize(uid, comp.Size, itemComp);
//         // This is also necessary
//         EnsureComp<MultiHandedItemComponent>(uid);
//         EnsureComp<CanEscapeInventoryComponent>(uid);
//         Dirty(uid, comp);
//     }
// }
