using Robust.Client.GameObjects;
using static Robust.Client.GameObjects.SpriteComponent;
using Content.Shared.Clothing;
using Content.Shared.Hands;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Content.Shared._CD.PaintCan;

namespace Content.Client._CD.PaintCan;

public sealed class PaintedVisualizerSystem : VisualizerSystem<CDPaintCanPaintedComponent>
{
    /// <summary>
    /// Visualizer for Paint which applies a shader and colors the entity.
    /// </summary>

    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CDPaintCanPaintedComponent, HeldVisualsUpdatedEvent>(OnHeldVisualsUpdated);
        SubscribeLocalEvent<CDPaintCanPaintedComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<CDPaintCanPaintedComponent, EquipmentVisualsUpdatedEvent>(OnEquipmentVisualsUpdated);
    }

    protected override void OnAppearanceChange(EntityUid uid, CDPaintCanPaintedComponent component, ref AppearanceChangeEvent args)
    {
        var shader = _protoMan.Index<ShaderPrototype>(component.ShaderName).Instance();

        if (args.Sprite == null)
            return;

        // What is this even doing? It's not even checking what the value is.
        if (!_appearance.TryGetData(uid, PaintVisuals.Painted, out bool isPainted))
            return;

        var sprite = args.Sprite;

        foreach (var spriteLayer in sprite.AllLayers)
        {
            if (spriteLayer is not Layer layer)
                continue;

            if (layer.Shader == null) // If shader isn't null we don't want to replace the original shader.
            {
                layer.Shader = shader;
                layer.Color = component.Color;
            }
        }
    }


    private void OnHeldVisualsUpdated(Entity<CDPaintCanPaintedComponent> ent, ref HeldVisualsUpdatedEvent args)
    {
        if (args.RevealedLayers.Count == 0)
            return;

        if (!TryComp(args.User, out SpriteComponent? spriteComp))
            return;

        var sprite = new Entity<SpriteComponent>(ent, spriteComp);
        foreach (var revealed in args.RevealedLayers)
        {
            if (!_sprite.LayerMapTryGet(sprite.AsNullable(), revealed, out var layer, false))
                continue;

            sprite.Comp.LayerSetShader(layer, ent.Comp.ShaderName);
            _sprite.LayerSetColor(sprite.AsNullable(), layer, ent.Comp.Color);
        }
    }

    private void OnEquipmentVisualsUpdated(Entity<CDPaintCanPaintedComponent> ent, ref EquipmentVisualsUpdatedEvent args)
    {
        if (args.RevealedLayers.Count == 0)
            return;

        if (!TryComp(args.Equipee, out SpriteComponent? spriteComp))
            return;

        var sprite = new Entity<SpriteComponent>(ent, spriteComp);

        foreach (var revealed in args.RevealedLayers)
        {
            if (!_sprite.LayerMapTryGet(sprite.AsNullable(), revealed, out var layer,false))
                continue;

            sprite.Comp.LayerSetShader(layer, ent.Comp.ShaderName);
            _sprite.LayerSetColor(sprite.AsNullable(), layer, ent.Comp.Color);
        }
    }

    private void OnShutdown(Entity<CDPaintCanPaintedComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp(ent, out SpriteComponent? sprite))
            return;

        if (Terminating(ent))
            return;

        ent.Comp.BeforeColor = sprite.Color;
        var shader = _protoMan.Index<ShaderPrototype>(ent.Comp.ShaderName).Instance();


        foreach (var spriteLayer in sprite.AllLayers)
        {
            if (spriteLayer is not Layer layer)
                continue;

            if (layer.Shader != shader) // If shader isn't same as one in component we need to ignore it.
                continue;

            layer.Shader = null;
            if (layer.Color == ent.Comp.Color) // If color isn't the same as one in component we don't want to change it.
                layer.Color = ent.Comp.BeforeColor;
        }
    }
}
