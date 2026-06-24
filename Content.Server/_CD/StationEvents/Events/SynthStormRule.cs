using Content.Server._CD.StationEvents.Components;
using Content.Server._CD.Traits;
using Content.Server.Chat.Managers;
using Content.Server.StationEvents.Events;
using Content.Shared.Chat;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._CD.StationEvents.Events;

public sealed partial class SynthStormRule : StationEventSystem<SynthStormRuleComponent>
{
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedRoleSystem _roles = default!;

    protected override void Started(EntityUid uid, SynthStormRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var synthQuery = EntityQueryEnumerator<SynthComponent>();
        var validSynths = new List<EntityUid>();
        while (synthQuery.MoveNext(out var ent, out var synthComp))
        {
            if (!HasComp<ActorComponent>(ent)) // Ensure we aren't picking someone who doesn't have an actor
                continue;

            validSynths.Add(ent);
        }

        if (validSynths.Count == 0)
            return;

        var chosen = RobustRandom.Pick(validSynths);
        if (!TryComp<ActorComponent>(chosen, out var actor))
            return; // This should never happen, so it's fine

        if(comp.SynthStormSound != null && _mind.TryGetMind(chosen, out var mindId, out _))
            _roles.MindPlaySound(mindId, comp.SynthStormSound);

        var msg = Loc.GetString("station-event-ion-storm-synth");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Yellow);
    }
}
