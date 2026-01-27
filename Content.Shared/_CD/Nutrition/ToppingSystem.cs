using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Examine;
using Content.Shared.Nutrition.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Interaction;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;

namespace Content.Shared.Nutrition.EntitySystems;

public sealed partial class ToppingSystem : EntitySystem
{
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private static LocId _addToppingVerb = "topping-verb-add-topping";
    private static LocId _portionTooSmall = "topping-verb-portion-too-small";
    private static LocId _noSpace = "topping-verb-no-space";
    private static LocId _examineTopping = "topping-examine";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ToppingSubmergerComponent, AfterInteractEvent>(OnAfterInteract, before: new[] {typeof(ReactionMixerSystem)});

        SubscribeLocalEvent<ToppableComponent, ExaminedEvent>(OnExamine);
        SubscribeLocalEvent<ToppableComponent, IngestedEvent>(OnIngested);
        SubscribeLocalEvent<ToppableComponent, GetVerbsEvent<ActivationVerb>>(AddToppingVerb);
    }

    private void OnAfterInteract(Entity<ToppingSubmergerComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach)
            return;

        if (!TryComp<ToppableComponent>(args.Target, out var toppable))
            return;

        if (toppable.Toppings.Count == 0)
            return;

        if (!TryComp<UtensilComponent>(args.Used, out var utensil))
            return;

        if ((utensil.Types & UtensilType.Spoon) == 0)
            return;

        toppable.Submerged.AddRange(toppable.Toppings);
        toppable.Toppings.Clear();
        Dirty(args.Target.Value, toppable);

        _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(args.User):entity} has submerged toppings of {ToPrettyString(ent.Owner):entity}");
    }

    private void OnExamine(Entity<ToppableComponent> ent, ref ExaminedEvent args)
    {
        var lastTopping = "";
        var allToppings = "";
        var lastToppingSubmerged = "";
        var allToppingsSubmerged = "";

        if (ent.Comp.Toppings.Count > 0)
        {
            lastTopping = ent.Comp.Toppings[^1];
            allToppings = string.Join(", ", ent.Comp.Toppings.GetRange(0, ent.Comp.Toppings.Count - 1));
        }

        if (ent.Comp.Submerged.Count > 0)
        {
            lastToppingSubmerged = ent.Comp.Submerged[^1];
            allToppingsSubmerged = string.Join(", ", ent.Comp.Submerged.GetRange(0, ent.Comp.Submerged.Count - 1));
        }


        var format = "";
        if (ent.Comp.Toppings.Count == 0 && ent.Comp.Submerged.Count == 0)
            return;
        if (ent.Comp.Toppings.Count == 0 && ent.Comp.Submerged.Count == 1)
            format = "one-submerged";
        else if (ent.Comp.Toppings.Count == 0 && ent.Comp.Submerged.Count > 1)
            format = "many-submerged";
        else if (ent.Comp.Toppings.Count == 1 && ent.Comp.Submerged.Count == 0)
            format = "one-topping";
        else if (ent.Comp.Toppings.Count == 1 && ent.Comp.Submerged.Count == 1)
            format = "one-topping-one-submerged";
        else if (ent.Comp.Toppings.Count == 1 && ent.Comp.Submerged.Count > 1)
            format = "one-topping-many-submerged";
        else if (ent.Comp.Toppings.Count > 1 && ent.Comp.Submerged.Count == 0)
            format = "many-topping";
        else if (ent.Comp.Toppings.Count > 1 && ent.Comp.Submerged.Count == 1)
            format = "many-topping-one-submerged";
        else if (ent.Comp.Toppings.Count > 1 && ent.Comp.Submerged.Count > 1)
            format = "many-topping-many-submerged";

        args.PushMarkup(Loc.GetString(_examineTopping, ("listTopping", allToppings), ("lastTopping", lastTopping), ("listSubmerged", allToppingsSubmerged), ("lastSubmerged", lastToppingSubmerged), ("format", format)));
    }

    private void OnIngested(Entity<ToppableComponent> ent, ref IngestedEvent args)
    {
        if (!_solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out var _, out var solution))
            return;

        if (solution.Volume > 0)
            return;

        ent.Comp.Toppings.Clear();
        ent.Comp.Submerged.Clear();
        Dirty(ent);
    }

    private void AddToppingVerb(Entity<ToppableComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanInteract)
            return;

        // Dont add more toppings than entity can hold
        if (ent.Comp.Toppings.Count + ent.Comp.Submerged.Count >= ent.Comp.MaxToppings)
            return;

        // Item must be held in hand
        if (args.Using == null)
            return;

        // Held item must be a topping
        if (!TryComp<ToppingComponent>(args.Using, out var topping))
            return;

        EntityUid user = args.User;
        Entity<ToppingComponent> toppingEntity = new(args.Using.Value, topping);

        var addToppingVerb = new ActivationVerb
        {
            Text = Loc.GetString(_addToppingVerb),
            Act = () =>
            {
                Log.Debug("aa");
                if (!_solution.TryGetSolution(ent.Owner, ent.Comp.Solution, out var toppableSolution))
                {
                    return;
                }

                Log.Debug("bb");
                if (!_solution.TryGetSolution(toppingEntity.Owner, toppingEntity.Comp.Solution, out var toppingSolution))
                {
                    return;
                }

                Log.Debug("zz");
                FixedPoint2 portionSize = 0;
                if (topping.PortionSize == null)
                    portionSize = toppingSolution.Value.Comp.Solution.Volume;
                else
                    portionSize = topping.PortionSize.Value;

                if (toppingSolution.Value.Comp.Solution.Volume < portionSize)
                {
                    _popup.PopupPredicted(Loc.GetString(_portionTooSmall), user, user);
                    return;
                }

                if (toppableSolution.Value.Comp.Solution.AvailableVolume < portionSize)
                {
                    _popup.PopupPredicted(Loc.GetString(_noSpace), user, user);
                    return;
                }

                if (ent.Comp.Toppings.Count + ent.Comp.Submerged.Count >= ent.Comp.MaxToppings)
                {
                    _popup.PopupPredicted(Loc.GetString(_noSpace), user, user);
                    return;
                }


                var splittedSolution = _solution.SplitSolution(toppingSolution.Value, portionSize);
                _solution.AddSolution(toppableSolution.Value, splittedSolution);


                ent.Comp.Toppings.Add(Name(toppingEntity.Owner));
                _audio.PlayPredicted(ent.Comp.InsertionSound, ent.Owner, null);
                _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(user):entity} has added {ToPrettyString(toppingEntity.Owner):entity} as topping to {ToPrettyString(ent.Owner):entity}");

                Dirty(ent);

                if (toppingSolution.Value.Comp.Solution.Volume == 0)
                    PredictedDel(toppingEntity.Owner);
            },
            Impact = LogImpact.Low,
        };

        addToppingVerb.Impact = LogImpact.Low;
        args.Verbs.Add(addToppingVerb);
    }
}

