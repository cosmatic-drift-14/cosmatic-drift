using Content.Server.Actions;
using Content.Server.Speech.EntitySystems;
using Content.Shared._CD.Species;
using Content.Shared.Nutrition;
using Content.Shared.Speech;
using Content.Shared.Storage;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Map;

namespace Content.Server._CD.Species;

public sealed class MouthStorageSystem : SharedMouthStorageSystem
{
    [Dependency] private readonly ReplacementAccentSystem _replacement = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly ActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MouthStorageComponent, ComponentInit>(OnMouthStorageInit);
        SubscribeLocalEvent<MouthStorageComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<MouthStorageComponent, IngestionAttemptEvent>(OnIngestAttempt);
    }

    private void OnMouthStorageInit(EntityUid uid, MouthStorageComponent component, ComponentInit args)
    {
        if (string.IsNullOrWhiteSpace(component.MouthProto))
            return;

        component.Mouth = _containerSystem.EnsureContainer<Container>(uid, MouthStorageComponent.MouthContainerId);
        component.Mouth.ShowContents = false;
        component.Mouth.OccludesLight = false;

        var mouth = Spawn(component.MouthProto, new EntityCoordinates(uid, -1, 0));
        _containerSystem.Insert(mouth, component.Mouth);
        component.MouthId = mouth;

        if (!string.IsNullOrWhiteSpace(component.OpenStorageAction) && component.Action == null)
            _actionsSystem.AddAction(uid, ref component.Action, component.OpenStorageAction, mouth);
    }

    // Force you to mumble if you have items in your mouth
    private void OnAccent(EntityUid uid, MouthStorageComponent component, AccentGetEvent args)
    {
        if (IsMouthBlocked(component))
            args.Message = _replacement.ApplyReplacements(args.Message, "mumble");
    }

    // Attempting to eat or drink anything with items in your mouth won't work
    private void OnIngestAttempt(EntityUid uid, MouthStorageComponent component, IngestionAttemptEvent args)
    {
        if (!IsMouthBlocked(component))
            return;

        if (!TryComp<StorageComponent>(component.MouthId, out var storage))
            return;

        var firstItem = storage.Container.ContainedEntities[0];
        args.Blocker = firstItem;
        args.Cancelled = true;
    }
}
