using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Shared.Anomaly.Components;
using Content.Shared.Anomaly.Effects.Components;
using Content.Shared.Mind;
using Robust.Shared.Map;
using Content.Server.Emp;
using Content.Server.Speech.Muting;
using Content.Server.Stunnable;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.SimpleStation14.Silicon.Components;
using Content.Shared.SimpleStation14.Silicon.Systems;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;


namespace Content.Server.Anomaly.Effects;

/// <summary>
/// This handles <see cref="PyroclasticAnomalyComponent"/> and the events from <seealso cref="AnomalySystem"/>
/// </summary>

public sealed class OldTVAnomalySystem : EntitySystem
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<OldTVAnomalyComponent, TVAnomalyPulseEvent>(OnPulse);
        SubscribeLocalEvent<OldTVAnomalyComponent, TVAnomalySupercriticalEvent>(OnSupercritical);
    }

    public void StunNearby(EntityUid uid, EntityCoordinates coordinates, float severity, float radius, ref TVEmpPulseEvent emp)
    {
        var players = new HashSet<Entity<MindComponent>>();
        _lookup.GetEntitiesInRange(coordinates, radius, players);

        foreach (var player in players)
        {
            if (!TryComp<StatusEffectsComponent>(uid, out var statusComp))
                return;

            var duration = emp.Duration / 1.5;
            var ent = player.Owner;
            var stackAmount = 1 + (int) (severity / 0.15f);
            _stun.TrySlowdown(uid, duration, true, _random.NextFloat(0.50f, 0.70f), _random.NextFloat(0.35f, 0.70f), statusComp);
            _status.TryAddStatusEffect<SeeingStaticComponent>(uid, SeeingStaticSystem.StaticKey, duration, true, statusComp);
        }
    }


    private void OnPulse(EntityUid uid, OldTVAnomalyComponent component, ref AnomalyPulseEvent args, ref TVEmpPulseEvent emp)
    {
        var xform = Transform(uid);
        var staticRadius = component.MaximumStaticRadius * args.Stability;
        StunNearby(uid, xform.Coordinates, args.Severity, staticRadius, emp.Duration);
    }

    private void OnSupercritical(EntityUid uid, OldTVAnomalyComponent component, ref AnomalySupercriticalEvent args, ref TVEmpPulseEvent emp)
    {
        var xform = Transform(uid);
        StunNearby(uid, xform.Coordinates, 1, component.CritStaticRadius, emp.Duration);
    }

}
