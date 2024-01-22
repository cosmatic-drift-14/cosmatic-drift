using Content.Server.Radio.EntitySystems;
using Content.Shared.Radio;
using Content.Shared.Roles;
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

        radio.SendRadioMessage(mob, Loc.GetString(NotifyTextKey), channel, mob);
    }
}
