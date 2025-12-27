using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CD.PaintCan;

/// <summary>
/// Component applied to target entity when painted with a spray paint <b>can</b>. Not to be confused with the wizden spray painter.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CDPaintCanPaintedComponent : Component
{
    /// <summary>
    ///  Color of the paint.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color Color = Color.FromHex("#2cdbd5");

    /// <summary>
    ///  Used to remove the color when component removed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color BeforeColor;

    /// <summary>
    /// If paint is enabled.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled;

    /// <summary>
    /// Name of the shader.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string ShaderName = "Greyscale";
}

[Serializable, NetSerializable]
public enum PaintVisuals : byte
{
    Painted,
}
