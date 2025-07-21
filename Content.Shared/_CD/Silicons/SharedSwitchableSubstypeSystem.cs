using Robust.Shared.Prototypes;

namespace Content.Shared._CD.Silicons;

/// <summary>
/// Shared behaviour for borg switchable subtype logic.
///
/// </summary>
public abstract class SharedBorgSwitchableSubtypeSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, BorgSubtypeSelectEvent>(OnSubtypeSelectEvent);
    }

    private void OnSubtypeSelectEvent(Entity<BorgSwitchableSubtypeComponent> ent, ref BorgSubtypeSelectEvent args)
    {
        throw new NotImplementedException();
    }

    protected virtual void UpdateEntityAppearance(Entity<BorgSwitchableSubtypeComponent> entity1,
        BorgSubtypePrototype borgSubtypePrototype)
    {
        throw new NotImplementedException();
    }

    protected virtual void SelectBorgSubtype(Entity<BorgSwitchableSubtypeComponent> ent,
        ProtoId<BorgSubtypePrototype> borgType)
    {
        throw new NotImplementedException();
    }
}

[ByRefEvent]
public sealed class BorgSubtypeSelectEvent : EntityEventArgs
{
    public EntityUid ChassisEntity;

    public ProtoId<BorgSubtypePrototype> BorgSubtypeId = "generic";
}
