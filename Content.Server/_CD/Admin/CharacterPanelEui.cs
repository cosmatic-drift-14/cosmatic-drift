using Content.Server.EUI;

namespace Content.Server._CD.Admin;

public sealed class CharacterPanelEui : BaseEui

{
    public CharacterPanelEui()
    {
        IoCManager.InjectDependencies(this);
    }
}
