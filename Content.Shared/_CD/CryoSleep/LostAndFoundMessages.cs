using Robust.Shared.Serialization;

namespace Content.Shared._CD.CryoSleep;

[Serializable, NetSerializable]
public sealed class LostAndFoundBuiState(Dictionary<string, List<LostItemData>> storedPlayers) : BoundUserInterfaceState
{
    public Dictionary<string, List<LostItemData>> StoredPlayers = storedPlayers;
}

[Serializable, NetSerializable]
public sealed class LostAndFoundRetrieveItemMessage(string playerName, string slotName, NetEntity user)
    : BoundUserInterfaceMessage
{
    public string PlayerName = playerName;
    public string SlotName = slotName;
    public NetEntity User = user;
}

[Serializable, NetSerializable]
public enum LostAndFoundUiKey : byte
{
    Key,
}
