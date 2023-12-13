using Content.Server.Stunnable.Components;
using Content.Shared.Audio;
using Content.Shared.Damage.Events;
using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Toggleable;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._CD_.Stunnable
{
    public sealed class TelebatonSystem : SharedTelebatonSystem
    {
        [Dependency] private readonly SharedItemSystem _item = default!;
        [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<TelebatonComponent, UseInHandEvent>(OnUseInHand);
            SubscribeLocalEvent<TelebatonComponent, ExaminedEvent>(OnExamined);
            SubscribeLocalEvent<TelebatonComponent, StaminaDamageOnHitAttemptEvent>(OnStaminaHitAttempt);
        }

//I'm not sure if I need this, I think I'll need to rework it, but ye
         private void OnStaminaHitAttempt(EntityUid uid, TelebatonComponent component, ref StaminaDamageOnHitAttemptEvent args)
        {
            if (!component.Activated ||
                !TryComp<BatteryComponent>(uid, out var battery) || !_battery.TryUseCharge(uid, component.EnergyPerUse, battery))
            {
                args.Cancelled = true;
                return;
            }
        }

        private void OnUseInHand(EntityUid uid, TelebatonComponent comp, UseInHandEvent args)
        {
            if (comp.Activated)
            {
                TurnOff(uid, comp);
            }
            else
            {
                TurnOn(uid, comp, args.User);
            }
        }

        private void TurnOff(EntityUid uid, TelebatonComponent comp)
        {
            if (!comp.Activated)
                return;

            if (TryComp<AppearanceComponent>(uid, out var appearance) &&
                TryComp<ItemComponent>(uid, out var item))
            {
                _item.SetHeldPrefix(uid, "off", item);
                _appearance.SetData(uid, ToggleVisuals.Toggled, false, appearance);
            }

            _audio.PlayPvs(comp.SparksSound, uid, AudioHelpers.WithVariation(0.25f));

            comp.Activated = false;
            Dirty(uid, comp);
        }

        private void TurnOn(EntityUid uid, TelebatonComponent comp, EntityUid user)
        {
            if (EntityManager.TryGetComponent<AppearanceComponent>(uid, out var appearance) &&
                EntityManager.TryGetComponent<ItemComponent>(uid, out var item))
            {
                _item.SetHeldPrefix(uid, "on", item);
                _appearance.SetData(uid, ToggleVisuals.Toggled, true, appearance);
            }

            _audio.PlayPvs(comp.SparksSound, uid, AudioHelpers.WithVariation(0.25f));
            comp.Activated = true;
            Dirty(uid, comp);
        }

        private void OnExamined(EntityUid uid, TelebatonComponent comp, ExaminedEvent args) //I may remove this and its dependency since I can't but .ftl's in the namespace.
        {
            var msg = comp.Activated
                ? Loc.GetString("comp-stunbaton-examined-on") //leaving this as comp-stunbaton for debugging
                : Loc.GetString("comp-stunbaton-examined-off");
            args.PushMarkup(msg);
        }
    }
}