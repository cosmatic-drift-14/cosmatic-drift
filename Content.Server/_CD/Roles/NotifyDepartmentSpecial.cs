using Content.Server.Radio.EntitySystems;
using Content.Server.Station.Components;
using Content.Server.Station.Systems;
using Content.Shared.Radio;
using Content.Shared.Roles;
using Content.Shared.Station.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server._CD.Roles;

public sealed partial class NotifyDepartmentSpecial : JobSpecial
{
    [DataField("notify_text", required: true)]
    public string NotifyTextKey { get; private set; } = string.Empty;

    [DataField("radio_channel", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<RadioChannelPrototype>))]
    public string RadioChannelKey { get; private set; } = string.Empty;

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
        var radio = entMan.System<RadioSystem>();
        var channel = prototypeManager.Index<RadioChannelPrototype>(RadioChannelKey);
        var stationManager = entMan.System<StationSystem>();

        // Notify people on all stations.
        foreach (var station in stationManager.GetStations())
        {
            // 2024.3.29:
            // This code is awful and has all sorts of problems but it is the best we can do because.
            // there does not exist a good way to broadcast a radio message everywhere. Hopefully this
            // can be improved whenever the radio refactor takes place.
            if (!entMan.TryGetComponent<StationDataComponent>(station, out var stationInfo))
                continue;
            var probablyStationGridOrCloseEnough = stationManager.GetLargestGrid((station, stationInfo));
            if (probablyStationGridOrCloseEnough == null)
                continue;
            radio.SendRadioMessage(station, Loc.GetString(NotifyTextKey), channel, probablyStationGridOrCloseEnough.Value);

            // Also let people on arrivals know, this causes ghosts to hear it twice
            radio.SendRadioMessage(station, Loc.GetString(NotifyTextKey), channel, mob);
        }
    }
}
