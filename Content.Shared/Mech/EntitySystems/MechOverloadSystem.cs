using Content.Shared.Actions;
using Content.Shared.Mech.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Damage;
using Content.Shared.Destructible;

namespace Content.Shared.Mech.EntitySystems;

/// <summary>
/// Handles all of the interactions, UI handling, and items shennanigans for <see cref="MechComponent"/>
/// </summary>
public sealed class MechOverloadSystem : EntitySystem
{
    private float _timer;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<MechOverloadComponent, MechOverloadEvent>(OnToggleOverload);
        SubscribeLocalEvent<MechOverloadComponent, DamageChangedEvent>(OnDamage);
    }

    private void OnToggleOverload(EntityUid uid, MechOverloadComponent comp, MechOverloadEvent args)
    {
        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(uid);
        if (!TryComp<MechComponent>(uid, out var mech))
            return;
        if (mech.Integrity <= comp.MinIng)
            return;
        if (comp.Overload == false)
        {
            _movementSpeedModifierSystem?.ChangeBaseSpeed(uid, 6, 6, 40, movementSpeed);
            mech.MechEnergyWaste += 20;
            comp.Overload = true;
            Spawn("EffectSparks", Transform(uid).Coordinates);
            _damageable.TryChangeDamage(uid, comp.DamagePerSpeed, ignoreResistances: true);
        }
        else
        {
            _movementSpeedModifierSystem?.ChangeBaseSpeed(uid, 3, 4, 40, movementSpeed);
            mech.MechEnergyWaste -= 20;
            comp.Overload = false;
        }
    }
    private void OnDamage(EntityUid uid, MechOverloadComponent component, DamageChangedEvent args)
    {
        var movementSpeed = EnsureComp<MovementSpeedModifierComponent>(uid);
        if (!TryComp<MechComponent>(uid, out var mech))
            return;
        if (mech.Integrity > component.MinIng)
            return;
        _movementSpeedModifierSystem?.ChangeBaseSpeed(uid, 3, 4, 40, movementSpeed);
        mech.MechEnergyWaste -= 20;
        component.Overload = false;
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _timer += frameTime;
        if (_timer < 1) return;
        _timer -= 1;

        var query = EntityQueryEnumerator<MechOverloadComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var overload, out var mech))
        {
            if (overload.Overload)
            {
                _damageable.TryChangeDamage(uid, overload.DamagePerSpeed, ignoreResistances: true);
            }
        }
    }
}

public sealed partial class MechOverloadEvent : InstantActionEvent
{
}

