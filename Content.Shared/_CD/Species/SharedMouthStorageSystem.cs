using Content.Shared.CombatMode;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.Standing;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;

namespace Content.Shared._CD.Species;

public abstract class SharedMouthStorageSystem : EntitySystem
{
    [Dependency] private readonly DumpableSystem _dumpableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MouthStorageComponent, DownedEvent>(DropAllContents);
        SubscribeLocalEvent<MouthStorageComponent, DisarmedEvent>(DropAllContents);
        SubscribeLocalEvent<MouthStorageComponent, DamageChangedEvent>(OnDamageModified);
        SubscribeLocalEvent<MouthStorageComponent, ExaminedEvent>(OnExamined);
    }

    protected bool IsMouthBlocked(MouthStorageComponent component)
    {
        if (!TryComp<StorageComponent>(component.MouthId, out var storage))
            return false;

        return storage.Container.ContainedEntities.Count > 0;
    }

    private void DropAllContents(EntityUid uid, MouthStorageComponent component, EntityEventArgs args)
    {
        if (component.MouthId == null)
            return;

        _dumpableSystem.DumpContents(component.MouthId.Value, uid, uid);
    }

    private void DropAllContents(EntityUid uid, MouthStorageComponent component, ref DisarmedEvent args)
    {
        if (component.MouthId == null)
            return;

        _dumpableSystem.DumpContents(component.MouthId.Value, uid, uid);
    }

    private void OnDamageModified(EntityUid uid, MouthStorageComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null
            || !args.DamageIncreased
            || args.DamageDelta.GetTotal() < component.SpitDamageThreshold)
            return;

        DropAllContents(uid, component, args);
    }

    // Other people can see if this person has items in their mouth.
    private void OnExamined(EntityUid uid, MouthStorageComponent component, ExaminedEvent args)
    {
        if (IsMouthBlocked(component))
        {
            var subject = Identity.Entity(uid, EntityManager);
            args.PushMarkup(Loc.GetString("mouth-storage-examine-condition-occupied", ("entity", subject)));
        }
    }
}
