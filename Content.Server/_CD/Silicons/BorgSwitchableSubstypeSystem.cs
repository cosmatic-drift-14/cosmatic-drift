using Content.Shared._CD.Silicons;
using Robust.Shared.Prototypes;

namespace Content.Server._CD.Silicons;

/// <summary>
/// Server-side logic that shouldn't be exposed to the client.
/// </summary>
public sealed class BorgSwitchableSubstypeSystem : SharedBorgSwitchableSubtypeSystem
{
    protected override void SelectBorgSubtype(Entity<BorgSwitchableSubtypeComponent> ent,
        ProtoId<BorgSubtypePrototype> borgType)
    {
        throw new NotImplementedException();
    }

}
