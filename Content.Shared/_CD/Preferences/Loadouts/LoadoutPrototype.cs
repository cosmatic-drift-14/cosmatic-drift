using Robust.Shared.Prototypes;

// ReSharper disable once CheckNamespace
namespace Content.Shared.Preferences.Loadouts;

public sealed partial class LoadoutPrototype
{
    [DataField]
    public EntProtoId? Brain { get; set; } = new();

    /// <summary>
    /// Any additional slots that this loadout will require, based on any pieces of clothing that will extend
    /// from the clothing. Should not include slots that the loadout already defined use for (i.e. the hat slot
    /// if this loadout is meant for a hat that has an extended radio)
    /// </summary>
    [DataField]
    public List<string>? AdditionalRequiredSlots { get; set; }
}
