using Content.Server.Connection.Whitelist;

namespace Content.Server._CD.Connection.Whitelist;

/// <summary>
/// Condition that matches if there are fewer than MaxPlayers un-whitelisted
/// </summary>
public sealed partial class ConditionWhitelistSlots : WhitelistCondition
{
    [DataField]
    public int MaximumPlayers = 2;
}
