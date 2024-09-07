using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Popups;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Examine;
using Content.Shared.Prayer;
using Content.Shared.Verbs;
using Robust.Shared.Player;

namespace Content.Server._CD.Engraving;

public sealed class EngraveableSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly QuickDialogSystem _quickDialog = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EngraveableComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<EngraveableComponent, GetVerbsEvent<ActivationVerb>>(AddEngraveVerb);
    }

    private void OnExamined(EntityUid uid, EngraveableComponent comp, ExaminedEvent args)
    {
        if (comp.EngravedMessage == string.Empty)
        {
            args.PushMarkup(Loc.GetString(comp.NoEngravingText));
            return;
        }

        args.PushMarkup(Loc.GetString(comp.HasEngravingText));
        args.PushMarkup(Loc.GetString(comp.EngravedMessage));
    }

    private void AddEngraveVerb(EntityUid uid, EngraveableComponent comp, GetVerbsEvent<ActivationVerb> args)
    {
        // First check if it's already been engraved. If it has, don't let them do it again.
        if (comp.EngravedMessage != string.Empty)
            return;

        // We need an actor to give the verb.
        if (!EntityManager.TryGetComponent(args.User, out ActorComponent? actor))
            return;

        // Make sure ghosts can't engrave stuff.
        if (!args.CanInteract)
            return;

        var engraveVerb = new ActivationVerb
        {
            Text = Loc.GetString("engraving-verb-engrave"),
            Act = () =>
            {
                _quickDialog.OpenDialog(actor.PlayerSession, Loc.GetString("engraving-verb-engrave"), Loc.GetString("engraving-popup-ui-message"), (string message) =>
                {
                    // If either the actor or comp have magically vanished
                    if (actor?.PlayerSession != null && actor.PlayerSession.AttachedEntity != null && HasComp<EngraveableComponent>(uid))
                    {
                        comp.EngravedMessage = message;
                        _popupSystem.PopupEntity(Loc.GetString(comp.EngraveSuccessMessage), actor.PlayerSession.AttachedEntity.Value, actor.PlayerSession, PopupType.Medium);
                        _adminLogger.Add(LogType.Action, LogImpact.Low, $"{ToPrettyString(actor.PlayerSession.AttachedEntity):player} engraved an item with message: {message}");
                    }
                });
            },
            Impact = LogImpact.Low,

        };
        engraveVerb.Impact = LogImpact.Low;
        args.Verbs.Add(engraveVerb);
    }
}
