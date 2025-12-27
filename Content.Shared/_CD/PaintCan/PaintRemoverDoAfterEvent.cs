using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.PaintCan;

[Serializable, NetSerializable]
public sealed partial class PaintRemoverDoAfterEvent : SimpleDoAfterEvent;
