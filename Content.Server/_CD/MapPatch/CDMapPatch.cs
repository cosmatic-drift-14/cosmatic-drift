/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Numerics;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.MapPatch;

public abstract partial class CDMapPatch;

/// <summary>
///     Represents a patch to a map in the form of spawning an entity.
///     This is applied in world space.
/// </summary>
[DataDefinition]
public sealed partial class CDSpawnEntityMapPatch : CDMapPatch
{
    [DataField]
    public EntProtoId Id;
    [DataField]
    public Vector2 WorldPosition;
    [DataField]
    public Angle WorldRotation;
}
