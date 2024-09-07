using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Server._CD.Engraving;

/// <summary>
/// Allows an items' description to be modified with an engraving
/// </summary>
[RegisterComponent]
public sealed partial class EngraveableComponent : Component
{
    /// <summary>
    /// Message given to user to notify them a message was sent
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public string EngravedMessage = string.Empty;

    /// <summary>
    /// The inspect text to use when there is no engraving
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public string NoEngravingText = "engraving-dogtags-no-message";

    /// <summary>
    /// The message to use when successfully engraving the item
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public string EngraveSuccessMessage = "engraving-dogtags-succeed";

    /// <summary>
    /// The inspect text to use when there is an engraving. The message will be shown seperately afterwards.
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public string HasEngravingText = "engraving-dogtags-has-message";
}
