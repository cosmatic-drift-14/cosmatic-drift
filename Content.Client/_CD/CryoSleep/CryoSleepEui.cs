using Content.Client.Eui;
using Content.Shared._CD.CryoSleep;
using JetBrains.Annotations;
using Robust.Client.Graphics;

namespace Content.Client._CD.CryoSleep;

[UsedImplicitly]
public sealed class CryoSleepEui : BaseEui
{
    private readonly AcceptCryoWindow _window;

    public CryoSleepEui()
    {
        _window = new AcceptCryoWindow();
        _window.OnChoice += choice => SendMessage(new AcceptCryoChoiceMessage(choice));
    }

    public override void Opened()
    {
        IoCManager.Resolve<IClyde>().RequestWindowAttention();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        _window.Close();
    }
}
