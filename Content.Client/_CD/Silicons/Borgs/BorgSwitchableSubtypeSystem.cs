using System.Linq;
using Content.Client.Silicons.Borgs;
using Content.Shared._CD.Silicons;
using Content.Shared._CD.Silicons.Borgs;
using Content.Shared.Silicons.Borgs.Components;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;

namespace Content.Client._CD.Silicons.Borgs;

/// <summary>
/// Primarily handles the appearance aspects of the borg subtype.
/// </summary>
public sealed class BorgSwitchableSubtypeSystem : SharedBorgSwitchableSubtypeSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly BorgSystem _borg = default!;
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    protected override void UpdateEntityAppearance(Entity<BorgSwitchableSubtypeComponent> entity, BorgSubtypePrototype borgSubtypePrototype)
    {
        // get our required components
        var (owner, _) = entity;
        if (!TryComp<SpriteComponent>(entity, out var chassisSprite))
            return;

        // remove all existing layers
        for (int i = chassisSprite.AllLayers.Count() - 1; i >= 0; i--)
        {
            _sprite.RemoveLayer((entity, chassisSprite), i);
        }

        for (int i = 0; i < borgSubtypePrototype.LayerData.Length; i++)
        {
            var layerData = borgSubtypePrototype.LayerData[i];

            layerData.RsiPath = borgSubtypePrototype.SpritePath?.ToString();
            _sprite.AddLayer((owner, chassisSprite), layerData, i);
        }

        if (TryComp<BorgChassisComponent>(entity, out var chassis))
        {
            _borg.SetMindStates(
                (entity.Owner, chassis),
                borgSubtypePrototype.SpriteHasMindState,
                borgSubtypePrototype.SpriteNoMindState);

            if (TryComp(entity, out AppearanceComponent? appearance))
            {
                // Queue update so state changes apply.
                _appearance.QueueUpdate(entity, appearance);
            }
        }

        base.UpdateEntityAppearance(entity, borgSubtypePrototype);
    }
}
