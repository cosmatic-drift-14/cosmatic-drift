using Content.Server.EUI;
using Content.Shared.Eui;
using Content.Shared._CD.CryoSleep;

namespace Content.Server._CD.CryoSleep;

public sealed class CryoSleepEui(EntityUid mind, CryoSleepSystem cryoSys) : BaseEui
{
    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is not AcceptCryoChoiceMessage choice ||
            choice.Button == AcceptCryoUiButton.Deny)
        {
            Close();
            return;
        }

        if (mind is { Valid: true })
        {
            cryoSys.CryoStoreBody(mind);
        }

        Close();
    }
}
