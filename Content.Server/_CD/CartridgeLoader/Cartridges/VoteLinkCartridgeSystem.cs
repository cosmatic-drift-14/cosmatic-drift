using Content.Server._CD.VoteLink;
using Content.Server.Access.Systems;
using Content.Server.CartridgeLoader;
using Content.Server.Station.Systems;
using Content.Shared._CD.CartridgeLoader.Cartridges;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.CartridgeLoader;

namespace Content.Server._CD.CartridgeLoader.Cartridges;

public sealed class VoteLinkCartridgeSystem : EntitySystem
{
    [Dependency] private readonly AccessReaderSystem _access = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly CartridgeLoaderSystem _cartridge = default!;
    [Dependency] private readonly IdCardSystem _id = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly VoteLinkSystem _vote = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoteLinkCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<VoteLinkCartridgeComponent, CartridgeMessageEvent>(OnMessage);
        SubscribeLocalEvent<VoteStartedEvent>(OnVoteStarted);
        SubscribeLocalEvent<VoteEndedEvent>(OnVoteEnded);
        SubscribeLocalEvent<VoteUpdatedEvent>(OnVoteUpdated);
    }

    private void OnMessage(Entity<VoteLinkCartridgeComponent> ent, ref CartridgeMessageEvent args)
    {
        if (args is not VoteLinkUiMessageEvent msg)
            return;

        if (ent.Comp.Station is not { } station)
            return;

        if (!HasVoteAccess(ent, GetEntity(args.LoaderUid)))
            return;

        // Can't create a vote if we're away from our PDA
        if (!_blocker.CanInteract(args.Actor, ent))
            return;

        // Forward the message to VoteLinkSystem
        _vote.HandleMessage(station, args.Actor, msg);
    }

    private bool HasVoteAccess(Entity<VoteLinkCartridgeComponent> ent, EntityUid loader)
    {
        if (!TryComp<AccessReaderComponent>(ent, out var comp))
            return true;

        if (!_id.TryGetIdCard(loader, out var idCard))
            return false;

        return _access.IsAllowed(idCard, ent, comp);
    }

    private void OnVoteStarted(VoteStartedEvent ev)
    {
        UpdateAllCartridges(ev.Station);
    }

    private void OnVoteEnded(VoteEndedEvent ev)
    {
        UpdateAllCartridges(ev.Station);
    }

    private void OnVoteUpdated(VoteUpdatedEvent ev)
    {
        UpdateAllCartridges(ev.Station);
    }

    private void OnUiReady(Entity<VoteLinkCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUI(ent, args.Loader);
    }

    private void UpdateAllCartridges(EntityUid station)
    {
        var query = EntityQueryEnumerator<VoteLinkCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (cartridge.LoaderUid is not { } loader || comp.Station != station)
                continue;
            UpdateUI((uid, comp), loader);
        }
    }

    private void UpdateUI(Entity<VoteLinkCartridgeComponent> ent, EntityUid loader)
    {
        if (_station.GetOwningStation(loader) is { } station)
            ent.Comp.Station = station;

        if (!TryComp<StationVoteDatabaseComponent>(ent.Comp.Station, out var database))
            return;

        var hasAccess = HasVoteAccess(ent, loader);
        var state = new VoteLinkUiState(database.ActiveVote, database.VoteHistory, database.Cooldown, hasAccess);
        _cartridge.UpdateCartridgeUiState(loader, state);
    }
}
