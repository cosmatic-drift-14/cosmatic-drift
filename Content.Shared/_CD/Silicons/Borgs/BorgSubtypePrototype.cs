using Content.Shared.Silicons.Borgs.Components;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CD.Silicons.Borgs;

/// <summary>
/// Information relating to a borg's subtype. Should be mostly cosmetic.
/// </summary>
[Prototype]
public sealed class BorgSubtypePrototype : IPrototype
{
    [ValidatePrototypeId<SoundCollectionPrototype>]
    private static readonly ProtoId<SoundCollectionPrototype> DefaultFootsteps = new("FootstepBorg");

    [IdDataField]
    public required string ID { get; set; }


    /// <summary>
    /// The parent borg type of this subtype.
    /// </summary>
    [DataField]
    public required string ParentType;


    /// <summary>
    /// Sprite path that the prototype's layer data will reference.
    /// </summary>
    [DataField]
    public ResPath? SpritePath;

    /// <summary>
    /// The visual layer data for the subtype.
    /// At the minimum should have definitions for each value of <see cref="BorgVisualLayers"/>.
    /// </summary>
    [DataField]
    public required PrototypeLayerData[] LayerData;

    [DataField]
    public required string SpriteHasMindState;
    [DataField]
    public required string SpriteNoMindState;

    [DataField]
    public required EntProtoId DummyPrototype;

    [DataField]
    public string PetSuccessString = "petting-success-generic-cyborg";
    [DataField]
    public string PetFailureString = "petting-failure-generic-cyborg";

    /// <summary>
    /// Sound specifier for footstep sounds created by this subtype.
    /// </summary>
    [DataField]
    public SoundSpecifier FootstepCollection { get; set; } = new SoundCollectionSpecifier(DefaultFootsteps);

}
