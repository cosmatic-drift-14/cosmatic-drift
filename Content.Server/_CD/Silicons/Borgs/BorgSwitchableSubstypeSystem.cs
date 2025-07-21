using Content.Shared._CD.Silicons;
using Content.Shared._CD.Silicons.Borgs;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Silicons.Borgs;

/// <summary>
/// Server-side logic that shouldn't be exposed to the client.
/// </summary>
public sealed class BorgSwitchableSubstypeSystem : SharedBorgSwitchableSubtypeSystem
{
    protected override void SelectBorgSubtype(Entity<BorgSwitchableSubtypeComponent> ent)
    {
        Log.Debug("Ran server code");
    }
}
