/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Numerics;
using Content.Server.Administration.Managers;
using Content.Server.Administration.Systems;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Shared.Administration;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Server._CD.MapPatch;

/// <summary>
///     This handles applying patches to a map.
///     A "patch" in this context is anything inheriting MapPatch, which allows for mostly arbitrary map changes.
/// </summary>
public sealed partial class MapPatchSystem : EntitySystem
{
    [Dependency] private readonly IAdminManager _admin = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;

    [Dependency] private readonly MapSystem _mapSys = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PreGameMapLoad>(OnPreMapLoad);
        SubscribeLocalEvent<PostGameMapLoad>(OnPostMapLoad);
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    // HACK: Exists because otherwise we can't possibly know what the map offset is after loading it.
    // The world would be better if i had more foresight years ago when writing the current map loading code.
    // Only valid between PreGameMapLoad (exclusive) and PostGameMapLoad (inclusive).
    // Map got refactored, this is my jank, weird way of redoing this.. hopefully.
    private Angle _iWroteABadTwoYearsAgo = default!;
    private Vector2 _mapOffset = Vector2.Zero;

    /// <summary>
    ///     The current patch set, containing patches added by the user for quick printout.
    /// </summary>
    /// <remarks>
    ///     This *could* be made per player, however I don't think that's important enough for the complexity it adds.
    /// </remarks>
    public List<CDMapPatch> Patchset = new();

    private void OnGetVerbs(GetVerbsEvent<Verb> msg)
    {
        if (!_admin.HasAdminFlag(msg.User, AdminFlags.Mapping))
            return;

        var proto = MetaData(msg.Target).EntityPrototype;

        // If it has no prototype we can't really spawn it with this system...
        if (proto is not { ID: { } protoId })
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
                    WorldRotation = _xform.GetWorldRotation(msg.Target),
                });
            },
        });
    }

    private void OnPreMapLoad(PreGameMapLoad ev)
    {
        _iWroteABadTwoYearsAgo = ev.Rotation;
        _mapOffset = ev.Offset;
    }

    private void OnPostMapLoad(PostGameMapLoad ev)
    {
        return; // TODO: cdrebase
        // if (ev.GameMap.Patchfile is not { } patchfile)
            // return;

        // Log.Info($"Applying patches to {ev.GameMap.ID} from {patchfile}.");
        //
        // var args = new MapPatchEvArgs(ev, _iWroteABadTwoYearsAgo);
        //
        // ApplyPatchToMap(args, patchfile);
    }

    /// <summary>
    ///     Applies the given patch file to a map.
    /// </summary>
    public void ApplyPatchToMap(MapPatchEvArgs args, ResPath patchfile)
    {
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
                        ApplySpawnPatch(args, spawnPatch);
                        break;
                    }
                default:
                    throw new NotImplementedException($"Haven't implemented support for {patch.GetType()} yet.");
            }
        }
    }

    private void ApplySpawnPatch(MapPatchEvArgs mapLoadEv, CDSpawnEntityMapPatch patch)
    {
        var worldCoords = new MapCoordinates(Vector2.Transform(patch.WorldPosition, Matrix3Helpers.CreateTransform(_mapOffset, mapLoadEv.rot)), mapLoadEv.Map);

        // Spawn isn't quite nice enough here, so to make sure we attach properly to any grids, we find it ourselves.
        if (!_mapMan.TryFindGridAt(worldCoords, out var grid, out var gridComp))
        {
            Log.Debug($"Spawning {patch.Id} at {worldCoords}");
            Spawn(patch.Id, worldCoords, rotation: patch.WorldRotation);
            return;
        }

        // I dislike this. Too verbose, but oh well.
        var gridLocal = _mapSys.WorldToLocal(grid, gridComp, worldCoords.Position);
        var coords = new EntityCoordinates(grid, gridLocal);
        Log.Debug($"Spawning {patch.Id} at {coords}/{worldCoords}");
        var ent = SpawnAtPosition(patch.Id, coords);
        _xform.SetWorldRotation(ent, patch.WorldRotation + _iWroteABadTwoYearsAgo);
    }
}

public struct MapPatchEvArgs
{
    public MapId Map;
    public Angle rot;

    public MapPatchEvArgs(PostGameMapLoad fromEv, Angle rotation)
    {
        Map = fromEv.Map;
        rot = rotation;
    }

    public MapPatchEvArgs(MapId map)
    {
        Map = map;
        rot = 0;
    }
}
