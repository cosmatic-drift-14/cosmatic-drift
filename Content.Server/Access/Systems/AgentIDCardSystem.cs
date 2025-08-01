using Content.Server.Access.Components;
using Content.Server.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Interaction;
using Content.Shared.StatusIcon;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Content.Shared.Roles;
using System.Diagnostics.CodeAnalysis;
using Content.Shared._CD.NanoChat; // CD
using Content.Server.Clothing.Systems;
using Content.Server.Implants;
using Content.Shared.Implants;
using Content.Shared.Inventory;
using Content.Shared.PDA;

namespace Content.Server.Access.Systems
{
    public sealed class AgentIDCardSystem : SharedAgentIdCardSystem
    {
        [Dependency] private readonly PopupSystem _popupSystem = default!;
        [Dependency] private readonly IdCardSystem _cardSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedNanoChatSystem _nanoChat = default!; // CD
        [Dependency] private readonly ChameleonClothingSystem _chameleon = default!;
        [Dependency] private readonly ChameleonControllerSystem _chamController = default!;

        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<AgentIDCardComponent, AfterInteractEvent>(OnAfterInteract);
            // BUI
            SubscribeLocalEvent<AgentIDCardComponent, AfterActivatableUIOpenEvent>(AfterUIOpen);
            SubscribeLocalEvent<AgentIDCardComponent, AgentIDCardNameChangedMessage>(OnNameChanged);
            SubscribeLocalEvent<AgentIDCardComponent, AgentIDCardJobChangedMessage>(OnJobChanged);
            SubscribeLocalEvent<AgentIDCardComponent, AgentIDCardJobIconChangedMessage>(OnJobIconChanged);
            SubscribeLocalEvent<AgentIDCardComponent, AgentIDCardNumberChangedMessage>(OnNumberChanged); // CD
            SubscribeLocalEvent<AgentIDCardComponent, InventoryRelayedEvent<ChameleonControllerOutfitSelectedEvent>>(OnChameleonControllerOutfitChangedItem);
        }

        // CD - Add number change handler
        private void OnNumberChanged(Entity<AgentIDCardComponent> ent, ref AgentIDCardNumberChangedMessage args)
        {
            if (!TryComp<NanoChatCardComponent>(ent, out var comp))
                return;

            _nanoChat.SetNumber((ent, comp), args.Number);
            Dirty(ent, comp);
        }

        private void OnChameleonControllerOutfitChangedItem(Entity<AgentIDCardComponent> ent, ref InventoryRelayedEvent<ChameleonControllerOutfitSelectedEvent> args)
        {
            if (!TryComp<IdCardComponent>(ent, out var idCardComp))
                return;

            _prototypeManager.TryIndex(args.Args.ChameleonOutfit.Job, out var jobProto);

            var jobIcon = args.Args.ChameleonOutfit.Icon ?? jobProto?.Icon;
            var jobName = args.Args.ChameleonOutfit.Name ?? jobProto?.Name ?? "";

            if (jobIcon != null)
                _cardSystem.TryChangeJobIcon(ent, _prototypeManager.Index(jobIcon.Value), idCardComp);

            if (jobName != "")
                _cardSystem.TryChangeJobTitle(ent, Loc.GetString(jobName), idCardComp);

            // If you have forced departments use those over the jobs actual departments.
            if (args.Args.ChameleonOutfit?.Departments?.Count > 0)
                _cardSystem.TryChangeJobDepartment(ent, args.Args.ChameleonOutfit.Departments, idCardComp);
            else if (jobProto != null)
                _cardSystem.TryChangeJobDepartment(ent, jobProto, idCardComp);

            // Ensure that you chameleon IDs in PDAs correctly. Yes this is sus...

            // There is one weird interaction: If the job / icon don't match the PDAs job the chameleon will be updated
            // to the PDAs IDs sprite but the icon and job title will not match. There isn't a way to get around this
            // really as there is no tie between job -> pda or pda -> job.

            var idSlotGear = _chamController.GetGearForSlot(args, "id");
            if (idSlotGear == null)
                return;

            var proto = _prototypeManager.Index(idSlotGear);
            if (!proto.TryGetComponent<PdaComponent>(out var comp, EntityManager.ComponentFactory))
                return;

            _chameleon.SetSelectedPrototype(ent, comp.IdCard);
        }

        private void OnAfterInteract(EntityUid uid, AgentIDCardComponent component, AfterInteractEvent args)
        {
            if (args.Target == null || !args.CanReach || !TryComp<AccessComponent>(args.Target, out var targetAccess) || !HasComp<IdCardComponent>(args.Target))
                return;

            if (!TryComp<AccessComponent>(uid, out var access) || !HasComp<IdCardComponent>(uid))
                return;

            var beforeLength = access.Tags.Count;
            access.Tags.UnionWith(targetAccess.Tags);
            var addedLength = access.Tags.Count - beforeLength;

            // CD - Copy NanoChat data if available
            if (TryComp<NanoChatCardComponent>(args.Target, out var targetNanoChat) &&
                TryComp<NanoChatCardComponent>(uid, out var agentNanoChat))
            {
                // First clear existing data
                _nanoChat.Clear((uid, agentNanoChat));

                // Copy the number
                if (_nanoChat.GetNumber((args.Target.Value, targetNanoChat)) is { } number)
                    _nanoChat.SetNumber((uid, agentNanoChat), number);

                // Copy all recipients and their messages
                foreach (var (recipientNumber, recipient) in _nanoChat.GetRecipients((args.Target.Value, targetNanoChat)))
                {
                    _nanoChat.SetRecipient((uid, agentNanoChat), recipientNumber, recipient);

                    if (_nanoChat.GetMessagesForRecipient((args.Target.Value, targetNanoChat), recipientNumber) is not
                        { } messages)
                        continue;

                    foreach (var message in messages)
                    {
                        _nanoChat.AddMessage((uid, agentNanoChat), recipientNumber, message);
                    }
                }
            }
            // End CD

            if (addedLength == 0)
            {
                _popupSystem.PopupEntity(Loc.GetString("agent-id-no-new", ("card", args.Target)), args.Target.Value, args.User);
                return;
            }

            Dirty(uid, access);

            if (addedLength == 1)
            {
                _popupSystem.PopupEntity(Loc.GetString("agent-id-new-1", ("card", args.Target)), args.Target.Value, args.User);
                return;
            }

            _popupSystem.PopupEntity(Loc.GetString("agent-id-new", ("number", addedLength), ("card", args.Target)), args.Target.Value, args.User);
            if (addedLength > 0)
                Dirty(uid, access);
        }

        private void AfterUIOpen(EntityUid uid, AgentIDCardComponent component, AfterActivatableUIOpenEvent args)
        {
            if (!_uiSystem.HasUi(uid, AgentIDCardUiKey.Key))
                return;

            if (!TryComp<IdCardComponent>(uid, out var idCard))
                return;

            // CD - Get current number if it exists
            uint? currentNumber = null;
            if (TryComp<NanoChatCardComponent>(uid, out var comp))
                currentNumber = comp.Number;

            var state = new AgentIDCardBoundUserInterfaceState(
                idCard.FullName ?? "",
                idCard.LocalizedJobTitle ?? "",
                idCard.JobIcon,
                currentNumber); // CD - Pass current number

            _uiSystem.SetUiState(uid, AgentIDCardUiKey.Key, state);
        }

        private void OnJobChanged(EntityUid uid, AgentIDCardComponent comp, AgentIDCardJobChangedMessage args)
        {
            if (!TryComp<IdCardComponent>(uid, out var idCard))
                return;

            _cardSystem.TryChangeJobTitle(uid, args.Job, idCard);
        }

        private void OnNameChanged(EntityUid uid, AgentIDCardComponent comp, AgentIDCardNameChangedMessage args)
        {
            if (!TryComp<IdCardComponent>(uid, out var idCard))
                return;

            _cardSystem.TryChangeFullName(uid, args.Name, idCard);
        }

        private void OnJobIconChanged(EntityUid uid, AgentIDCardComponent comp, AgentIDCardJobIconChangedMessage args)
        {
            if (!TryComp<IdCardComponent>(uid, out var idCard))
                return;

            if (!_prototypeManager.TryIndex(args.JobIconId, out var jobIcon))
                return;

            _cardSystem.TryChangeJobIcon(uid, jobIcon, idCard);

            if (TryFindJobProtoFromIcon(jobIcon, out var job))
                _cardSystem.TryChangeJobDepartment(uid, job, idCard);
        }

        private bool TryFindJobProtoFromIcon(JobIconPrototype jobIcon, [NotNullWhen(true)] out JobPrototype? job)
        {
            foreach (var jobPrototype in _prototypeManager.EnumeratePrototypes<JobPrototype>())
            {
                if (jobPrototype.Icon == jobIcon.ID)
                {
                    job = jobPrototype;
                    return true;
                }
            }

            job = null;
            return false;
        }
    }
}
