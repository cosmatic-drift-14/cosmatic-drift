using Robust.Shared.Prototypes;

namespace Content.Shared._CD.Silicons;

/// <summary>
/// Information relating to a borg's subtype. Should be mostly cosmetic.
/// </summary>
[Prototype]
public sealed class BorgSubtypePrototype : IPrototype
{
    [IdDataField]
    public required string ID { get; set; }
}
