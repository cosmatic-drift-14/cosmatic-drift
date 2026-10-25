using Content.Server.Clothing.Systems;
using Content.Server.Implants;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Clothing.Components;
using Content.Shared.Implants;
using Content.Shared.Inventory;
using Content.Shared.PDA;

namespace Content.Server.Access.Systems;

/// <inheritdoc />
public sealed partial class AgentIdCardSystem : SharedAgentIdCardSystem
{
    [Dependency] private SharedIdCardSystem _card = default!;
    [Dependency] private ChameleonClothingSystem _chameleon = default!;
    [Dependency] private ChameleonControllerSystem _chamController = default!;

    [SubscribeLocalEvent]
    private void OnChameleonControllerOutfitChangedItem(Entity<AgentIDCardComponent> ent, ref InventoryRelayedEvent<ChameleonControllerOutfitSelectedEvent> args)
    {
        if (!TryComp<IdCardComponent>(ent, out var idCardComp))
            return;
            
        [Dependency] private PopupSystem _popupSystem = default!;
        [Dependency] private IdCardSystem _cardSystem = default!;
        [Dependency] private UserInterfaceSystem _uiSystem = default!;
        [Dependency] private IPrototypeManager _prototypeManager = default!;
        [Dependency] private ChameleonClothingSystem _chameleon = default!;
        [Dependency] private ChameleonControllerSystem _chamController = default!;
        [Dependency] private LockSystem _lock = default!;
        [Dependency] private SharedJobStatusSystem _jobStatus = default!;

        [Dependency] private SharedNanoChatSystem _nanoChat = default!; // CD

        ProtoMan.Resolve(args.Args.ChameleonOutfit.Job, out var jobProto);

        var jobIcon = args.Args.ChameleonOutfit.Icon ?? jobProto?.Icon;
        var jobName = args.Args.ChameleonOutfit.Name ?? jobProto?.Name ?? "";

        if (jobIcon != null)
            _card.TryChangeJobIcon(ent, ProtoMan.Index(jobIcon.Value), idCardComp);

        if (jobName != "")
            _card.TryChangeJobTitle(ent, Loc.GetString(jobName), idCardComp);

        // If you have forced departments use those over the jobs actual departments.
        if (args.Args.ChameleonOutfit.Departments?.Count > 0)
            _card.TryChangeJobDepartment(ent, args.Args.ChameleonOutfit.Departments, idCardComp);
        else if (jobProto != null)
            _card.TryChangeJobDepartment(ent, jobProto, idCardComp);

        // Ensure that you chameleon IDs in PDAs correctly. Yes this is sus...

        // There is one weird interaction: If the job / icon don't match the PDAs job the chameleon will be updated
        // to the PDAs IDs sprite but the icon and job title will not match. There isn't a way to get around this
        // really as there is no tie between job -> pda or pda -> job.

        var idSlotGear = _chamController.GetGearForSlot(args, "id");
        if (idSlotGear == null)
            return;

        var proto = ProtoMan.Index(idSlotGear);
        if (!proto.TryComp<PdaComponent>(out var comp, EntityManager.ComponentFactory))
            return;

        if (TryComp<ChameleonClothingComponent>(ent, out var chameleonComp) && chameleonComp.CanBeSetByController)
            _chameleon.SetSelectedPrototype(ent, comp.IdCard, component: chameleonComp);
    }
}
