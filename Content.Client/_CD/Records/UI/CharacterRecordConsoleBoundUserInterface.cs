using Content.Shared._CD.Records;
using Content.Shared.StationRecords;
using JetBrains.Annotations;

namespace Content.Client._CD.Records.UI;

[UsedImplicitly]
public sealed class CharacterRecordConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables] private CharacterRecordViewer? _window;

    public CharacterRecordConsoleBoundUserInterface(EntityUid owner, Enum key)
        : base(owner, key)
    {
    }

    protected override void UpdateState(BoundUserInterfaceState baseState)
    {
        base.UpdateState(baseState);
        if (baseState is not CharacterRecordConsoleState state)
            return;

        _window?.UpdateState(state);
    }

    protected override void Open()
    {
        base.Open();

        _window = new();
        _window.OnClose += Close;
        _window.OnKeySelected += ent =>
        {
            SendMessage(new CharacterRecordConsoleSelectMsg(ent));
        };

        _window.OnFiltersChanged += (ty, txt) =>
        {
            if (txt == null)
                SendMessage(new CharacterRecordsConsoleFilterMsg(null));
            else
                SendMessage(new CharacterRecordsConsoleFilterMsg(new StationRecordsFilter(ty, txt)));
        };

        _window.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
    }
}
