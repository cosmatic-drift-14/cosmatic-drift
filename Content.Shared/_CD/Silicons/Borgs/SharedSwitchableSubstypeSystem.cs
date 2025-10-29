using Content.Shared.Interaction;
using Content.Shared.Interaction.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._CD.Silicons.Borgs;

/// <summary>
/// Shared behaviour for borg switchable subtype logic.
/// Essentially a reimplementation of <see cref="SharedBorgSwitchableTypeSystem"/> specifically for cosmetic functions.
/// </summary>
public abstract class SharedBorgSwitchableSubtypeSystem : EntitySystem
{
    [Dependency] private readonly InteractionPopupSystem _interactionPopup = default!;
    [Dependency] protected readonly IPrototypeManager Prototypes = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, MapInitEvent>(OnMapInit); // make sure that our subtype is selected first
        SubscribeLocalEvent<BorgSwitchableSubtypeComponent, AfterBorgTypeSelectEvent>(OnBorgTypeSelect);

        Subs.BuiEvents<BorgSwitchableTypeComponent>(BorgSwitchableTypeUiKey.SelectBorgType,
            sub =>
            {
                sub.Event<BorgSelectSubtypeMessage>(SelectSubtypeMessageHandler);
            });

        base.Initialize();
    }

    private void OnMapInit(Entity<BorgSwitchableSubtypeComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.BorgSubtype != null)
        {
            SelectBorgSubtype(ent);
        }
    }

    private void OnBorgTypeSelect(Entity<BorgSwitchableSubtypeComponent> ent, ref AfterBorgTypeSelectEvent args)
    {
        if (!ent.Comp.BorgSubtype.HasValue)
            return;

        SelectBorgSubtype(ent);
    }

    protected virtual void SelectBorgSubtype(Entity<BorgSwitchableSubtypeComponent> ent)
    {
        UpdateEntityAppearance(ent);
    }

    private void UpdateEntityAppearance(Entity<BorgSwitchableSubtypeComponent> entity)
    {
        if (!Prototypes.TryIndex(entity.Comp.BorgSubtype, out var subtypePrototype))
            return;

        UpdateEntityAppearance(entity,  subtypePrototype);
    }

    protected virtual void UpdateEntityAppearance(Entity<BorgSwitchableSubtypeComponent> entity,
        BorgSubtypePrototype borgSubtypePrototype)
    {
        if (TryComp(entity, out InteractionPopupComponent? popup))
        {
            _interactionPopup.SetInteractSuccessString((entity.Owner, popup), borgSubtypePrototype.PetSuccessString);
            _interactionPopup.SetInteractFailureString((entity.Owner, popup), borgSubtypePrototype.PetFailureString);
        }

        if (TryComp(entity, out FootstepModifierComponent? footstepModifier))
        {
            footstepModifier.FootstepSoundCollection = borgSubtypePrototype.FootstepCollection;
        }
    }

    private void SelectSubtypeMessageHandler(EntityUid uid, BorgSwitchableTypeComponent borgSwitchableTypeComponent, BorgSelectSubtypeMessage args)
    {
        if (!TryComp<BorgSwitchableSubtypeComponent>(uid, out var subtypeComp))
            return;

        subtypeComp.BorgSubtype = args.Subtype;
        Dirty(uid, subtypeComp);
    }
}
