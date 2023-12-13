using Content.Server.Stunnable.Components;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._CD.Stunnable;

public abstract class SharedTelebatonSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TelebatonComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
    }

    private void OnGetMeleeDamage(EntityUid uid, TelebatonComponent component, ref GetMeleeDamageEvent args)
    {
        if (!component.Activated)
            return;

        // Don't apply damage if it's activated; just do stamina damage.
        args.Damage = new DamageSpecifier();
    }
}