/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 * This Source Code Form is "Incompatible With Secondary Licenses", as
 * defined by the Mozilla Public License, v. 2.0.
 */

using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Map;

namespace Content.Server._CD.MapPatch;

/// <summary>
///     This handles applying patches to a map.
///     A "patch" in this context is anything inheriting MapPatch, which allows for mostly arbitrary map changes.
/// </summary>
public sealed partial class MapPatchSystem : EntitySystem
{
    [Dependency] private readonly ChatManager _chat = default!;
    [Dependency] private readonly TransformSystem _xform = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly MapSystem _mapSys = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PostGameMapLoad>(OnPostMapLoad);
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    /// <summary>
    ///     The current patch set, containing patches added by the user for quick printout.
    /// </summary>
    /// <remarks>
    ///     This *could* be made per player, however I don't think that's important enough for the complexity it adds.
    /// </remarks>
    public List<CDMapPatch> Patchset = new();

    private void OnGetVerbs(GetVerbsEvent<Verb> msg)
    {
        var proto = MetaData(msg.Target).EntityPrototype;

        // If it has no prototype we can't really spawn it with this system...
        if (proto is not {ID: { } protoId})
            return;

        msg.Verbs.Add(new Verb()
        {
            Category = VerbCategory.Debug,
            Text = Loc.GetString("cd-create-map-patch-verb"),
            Message = Loc.GetString("cd-create-map-patch-verb-desc"),
            Act = () =>
            {
                Patchset.Add(new CDSpawnEntityMapPatch
                {
                    Id = new(protoId),
                    WorldPosition = _xform.GetWorldPosition(msg.Target),
                    WorldRotation =  _xform.GetWorldRotation(msg.Target),
                });
            },
        });
    }

    private void OnPostMapLoad(PostGameMapLoad ev)
    {
        if (ev.GameMap.Patchfile is not { } patchfile)
            return;

        Log.Info($"Applying patches to {ev.GameMap.ID} from {patchfile}.");

        if (!TryLoadPatchfile(patchfile, out var patches))
        {
            // Yell at the players that the patch failed to load + console error.
            Log.Error("Failed to load map patches!");
            _chat.DispatchServerAnnouncement(
                "Map patches failed to load and some important objects may be missing, bug your local coder to fix them.",
                Color.Red
                );
            return;
        }

        foreach (var patch in patches)
        {
            switch (patch)
            {
                case CDSpawnEntityMapPatch spawnPatch:
                {
                    ApplySpawnPatch(ev, spawnPatch);
                    break;
                }
                default:
                    throw new NotImplementedException($"Haven't implemented support for {patch.GetType()} yet.");
            }
        }
    }

    private void ApplySpawnPatch(PostGameMapLoad mapLoadEv, CDSpawnEntityMapPatch patch)
    {
        var worldCoords = new MapCoordinates(patch.WorldPosition, mapLoadEv.Map);

        // Spawn isn't quite nice enough here, so to make sure we attach properly to any grids, we find it ourself.
        if (!_mapMan.TryFindGridAt(worldCoords, out var grid, out var gridComp))
        {
            Spawn(patch.Id, worldCoords, rotation: patch.WorldRotation);
            return;
        }

        // I dislike this. Too verbose, but oh well.
        var gridLocal = _mapSys.WorldToLocal(grid, gridComp, worldCoords.Position);
        var coords = new EntityCoordinates(grid, gridLocal);
        var ent = SpawnAtPosition(patch.Id, coords);
        _xform.SetWorldRotation(ent, patch.WorldRotation);
    }
}
