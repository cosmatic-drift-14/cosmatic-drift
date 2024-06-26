/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Robust.Shared.ContentPack;
using Robust.Shared.Exceptions;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Utility;

namespace Content.Server._CD.MapPatch;

public sealed partial class MapPatchSystem
{
    [Dependency] private readonly IResourceManager _res = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly IRuntimeLog _runtime = default!;

    /// <summary>
    ///     Attempt to load a patchfile, and provide a list of patches to make to the map.
    /// </summary>
    private bool TryLoadPatchfile(ResPath patch, [NotNullWhen(true)] out List<CDMapPatch>? patches)
    {
        patches = null;

        DataNodeDocument[] documents;
        {
            // Disposable resources inside the block get disposed at the end, so this ensures the streamreader doesn't
            // live for the entirety of us loading the patchfile.
            if (!_res.TryContentFileRead(patch, out var stream))
            {
                Log.Error($"Patch file {patch} is unreadable or missing.");
                return false;
            }

            using var reader = new StreamReader(stream, EncodingHelpers.UTF8);
            documents = DataNodeParser.ParseYamlStream(reader).ToArray();
        }

        if (documents.Length > 1)
        {
            Log.Error("Patch file contained more than one document, treating as malformed!");
            return false;
        }

        if (!documents.TryGetValue(0, out var document))
        {
            Log.Error(
                "Patch file contained no documents, and is likely empty. Remove the line from the map or add patches to it, treating as malformed.");
            return false;
        }

        if (document.Root is not SequenceDataNode seq)
        {
            Log.Error($"Patchfiles must be a list of patches, the root node is a {document.Root.GetType()} instead.");
            return false;
        }

        // SerializationManager is stinky and will throw on read fail instead of being nice.
        try
        {
            patches = _serialization.Read<List<CDMapPatch>>(seq, notNullableOverride: false);
            return true;
        }
        catch (Exception e)
        {
            Log.Error("Okay serialization did something bad and I can't make its errors any prettier so here you go:");
            _runtime.LogException(e, "map patcher");
        }

        // Well, serialization must've failed.
        return false;
    }

    public string PatchToText()
    {
        var nodes = _serialization.WriteValue(Patchset, notNullableOverride: true);
        return nodes.ToString();
    }
}
