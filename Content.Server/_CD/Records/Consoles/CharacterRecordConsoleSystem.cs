using System.Linq;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Shared._CD.Records;
using Content.Shared.StationRecords;
using Robust.Server.GameObjects;

namespace Content.Server._CD.Records.Consoles;

public sealed class CharacterRecordConsoleSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly CharacterRecordsSystem _characterRecords = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CharacterRecordConsoleComponent, BoundUIOpenedEvent>((uid, component, _) => UpdateUi(uid, component));
        SubscribeLocalEvent<CharacterRecordConsoleComponent, CharacterRecordsModifiedEvent>((uid, component, _) => UpdateUi(uid, component));
        SubscribeLocalEvent<CharacterRecordConsoleComponent, CharacterRecordConsoleSelectMsg>(OnKeySelect);
        SubscribeLocalEvent<CharacterRecordConsoleComponent, CharacterRecordsConsoleFilterMsg>(OnFilterApplied);
    }

    private void OnFilterApplied(EntityUid entity, CharacterRecordConsoleComponent console,
        CharacterRecordsConsoleFilterMsg msg)
    {
        console.Filter = msg.Filter;
        UpdateUi(entity, console);
    }

    private void OnKeySelect(EntityUid entity, CharacterRecordConsoleComponent console,
        CharacterRecordConsoleSelectMsg msg)
    {
        console.Selected = msg.Key;
        UpdateUi(entity, console);
    }

    private void UpdateUi(EntityUid entity, CharacterRecordConsoleComponent? console = null)
    {
        if (!Resolve(entity, ref console))
            return;

        var station = _stationSystem.GetOwningStation(entity);
        if (!TryComp<StationRecordsComponent>(station, out _) ||
            !TryComp<CharacterRecordsComponent>(station, out _))
        {
            SendState(entity, new CharacterRecordConsoleState { ConsoleType = console.ConsoleType });
            return;
        }

        var characterRecords = _characterRecords.QueryRecords(station.Value);
        var names = characterRecords
            .Where(kv =>
            {
                if (console.Filter != null)
                {
                    return !IsSkippedRecord(console.Filter, kv.Value);
                }

                return true;
            })
            .Select(r => (_entityManager.GetNetEntity(r.Key), $"{r.Value.Name} ({r.Value.JobTitle})"))
            .ToDictionary();

        var record = console.Selected == null ? null : characterRecords[_entityManager.GetEntity(console.Selected.Value)];

        SendState(entity,
            new CharacterRecordConsoleState
            {
                ConsoleType = console.ConsoleType,
                RecordListing = names,
                Selected = console.Selected,
                SelectedRecord = record,
                Filter = console.Filter,
            });
    }

    private void SendState(EntityUid entity, CharacterRecordConsoleState state)
    {
        _userInterface.TrySetUiState(entity, CharacterRecordConsoleKey.Key, state);
    }

    // The next two methods where almost copied verbatim from GeneralStationRecordConsoleSystem
    private static bool IsSkippedRecord(GeneralStationRecordsFilter filter,
        FullCharacterRecords record)
    {
        bool isFilter = filter.Value.Length > 0;

        if (!isFilter)
            return false;

        var filterLowerCaseValue = filter.Value.ToLower();

        return filter.Type switch
        {
            GeneralStationRecordFilterType.Name =>
                !record.Name.ToLower().Contains(filterLowerCaseValue),
            GeneralStationRecordFilterType.Prints => record.Fingerprint != null
                && IsFilterWithSomeCodeValue(record.Fingerprint, filterLowerCaseValue),
            GeneralStationRecordFilterType.DNA => record.DNA != null
                && IsFilterWithSomeCodeValue(record.DNA, filterLowerCaseValue),
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid Character Record filter type"),
        };
    }

    private static bool IsFilterWithSomeCodeValue(string value, string filter)
    {
        return !value.ToLower().StartsWith(filter);
    }
}
