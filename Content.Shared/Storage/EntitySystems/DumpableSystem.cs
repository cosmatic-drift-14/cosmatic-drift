using System.Linq;
using Content.Shared.Disposal;
using Content.Shared.Disposal.Components;
using Content.Shared.Disposal.Unit;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Placeable;
using Content.Shared.Storage.Components;
using Content.Shared.Verbs;
using JetBrains.Annotations;
using Content.Shared.Item;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Storage.EntitySystems;

public sealed class DumpableSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedDisposalUnitSystem _disposalUnitSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;

    private EntityQuery<ItemComponent> _itemQuery;

    public override void Initialize()
    {
        base.Initialize();
        _itemQuery = GetEntityQuery<ItemComponent>();
        SubscribeLocalEvent<DumpableComponent, AfterInteractEvent>(OnAfterInteract, after: new[]{ typeof(SharedEntityStorageSystem) });
        SubscribeLocalEvent<DumpableComponent, GetVerbsEvent<AlternativeVerb>>(AddDumpVerb);
        SubscribeLocalEvent<DumpableComponent, GetVerbsEvent<UtilityVerb>>(AddUtilityVerbs);
        SubscribeLocalEvent<DumpableComponent, DumpableDoAfterEvent>(OnDoAfter);
    }

    private void OnAfterInteract(EntityUid uid, DumpableComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || args.Handled)
            return;

        if (!HasComp<DisposalUnitComponent>(args.Target) && !HasComp<PlaceableSurfaceComponent>(args.Target))
            return;

        if (!TryComp<StorageComponent>(uid, out var storage))
            return;

        if (!storage.Container.ContainedEntities.Any())
            return;

        StartDoAfter(uid, args.Target.Value, args.User, component);
        args.Handled = true;
    }

    private void AddDumpVerb(EntityUid uid, DumpableComponent dumpable, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<StorageComponent>(uid, out var storage) || !storage.Container.ContainedEntities.Any())
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                StartDoAfter(uid, args.Target, args.User, dumpable);//Had multiplier of 0.6f
            },
            Text = Loc.GetString("dump-verb-name"),
            Icon = new SpriteSpecifier.Texture(new ("/Textures/Interface/VerbIcons/drop.svg.192dpi.png")),
        };
        args.Verbs.Add(verb);
    }

    private void AddUtilityVerbs(EntityUid uid, DumpableComponent dumpable, GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (!TryComp<StorageComponent>(uid, out var storage) || !storage.Container.ContainedEntities.Any())
            return;

        if (HasComp<DisposalUnitComponent>(args.Target))
        {
            UtilityVerb verb = new()
            {
                Act = () =>
                {
                    StartDoAfter(uid, args.Target, args.User, dumpable);
                },
                Text = Loc.GetString("dump-disposal-verb-name", ("unit", args.Target)),
                IconEntity = GetNetEntity(uid)
            };
            args.Verbs.Add(verb);
        }

        if (HasComp<PlaceableSurfaceComponent>(args.Target))
        {
            UtilityVerb verb = new()
            {
                Act = () =>
                {
                    StartDoAfter(uid, args.Target, args.User, dumpable);
                },
                Text = Loc.GetString("dump-placeable-verb-name", ("surface", args.Target)),
                IconEntity = GetNetEntity(uid)
            };
            args.Verbs.Add(verb);
        }
    }

    private void StartDoAfter(EntityUid storageUid, EntityUid targetUid, EntityUid userUid, DumpableComponent dumpable)
    {
        if (!TryComp<StorageComponent>(storageUid, out var storage))
            return;

        var delay = storage.Container.ContainedEntities.Count * (float) dumpable.DelayPerItem.TotalSeconds * dumpable.Multiplier;

        _doAfterSystem.TryStartDoAfter(new DoAfterArgs(EntityManager, userUid, delay, new DumpableDoAfterEvent(), storageUid, target: targetUid, used: storageUid)
        {
            BreakOnMove = true,
            NeedHand = true,
        });
    }

    private void OnDoAfter(EntityUid uid, DumpableComponent component, DumpableDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        DumpContents(uid, args.Args.Target, args.Args.User, component);
        // CD: Split OnAfterInteract into two methods to allow dumping that doesn't require a verb, which is required
        // for the functionality of Rodentia mouth storage being spilled when damaged. See DumpContents.
    }

    [PublicAPI]
    public void DumpContents(EntityUid uid, EntityUid? target, EntityUid user, DumpableComponent? component = null)
    {
        // CD: Split OnAfterInteract into two methods to allow dumping that doesn't require a verb, which is required
        // for the functionality of Rodentia mouth storage being spilled when damaged.
        if (!TryComp<StorageComponent>(uid, out var storage)
            || !Resolve(uid, ref component))
            return;

        if (storage.Container.ContainedEntities.Count == 0)
            return;

        var dumpQueue = new Queue<EntityUid>(storage.Container.ContainedEntities);

        var dumped = false;

        if (target.HasValue && HasComp<DisposalUnitComponent>(target))
        {
            dumped = true;

            foreach (var entity in dumpQueue)
            {
                _disposalUnitSystem.DoInsertDisposalUnit(target.Value, entity, user);
            }
        }
        else if (HasComp<PlaceableSurfaceComponent>(target))
        {
            dumped = true;

            var (targetPos, targetRot) = _transformSystem.GetWorldPositionRotation(target.Value);

            foreach (var entity in dumpQueue)
            {
                _transformSystem.SetWorldPositionRotation(entity, targetPos + _random.NextVector2Box() / 4, targetRot);
            }
        }
        else
        {
            var targetPos = _transformSystem.GetWorldPosition(uid);

            foreach (var entity in dumpQueue)
            {
                var transform = Transform(entity);
                _transformSystem.SetWorldPositionRotation(entity, targetPos + _random.NextVector2Box() / 4, _random.NextAngle(), transform);
            }
        }

        if (dumped)
        {
            _audio.PlayPredicted(component.DumpSound, uid, user);
        }
    }
}
