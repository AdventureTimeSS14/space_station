using Content.Server.Atmos.EntitySystems;
using Content.Shared.Anomaly.Components;
using Content.Shared.Anomaly.Effects.Components;
using Content.Shared.Mind;
using Robust.Shared.Map;
using Content.Server.Stunnable;
using Content.Shared.SimpleStation14.Silicon.Components;
using Content.Shared.SimpleStation14.Silicon.Systems;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;
using Content.Server.Electrocution;
using Robust.Shared.Timing;
using Content.Shared.ADT.Components;
using Content.Shared.Anomaly.Effects;

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
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;


    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<OldTVAnomalyComponent, TVAnomalyPulseEvent>(OnPulse);
        SubscribeLocalEvent<OldTVAnomalyComponent, TVAnomalySupercriticalEvent>(OnSupercritical);
    }
    public void StunNearby(EntityUid uid, EntityCoordinates coordinates, TimeSpan time, float severity, float radius)
    {
        var players = new HashSet<Entity<MindComponent>>();
        _lookup.GetEntitiesInRange(coordinates, radius, players);

        foreach (var player in players)
        {
            if (!TryComp<StatusEffectsComponent>(uid, out var statusComp))
                return;

            var duration = TimeSpan.FromSeconds(10);
            _stun.TrySlowdown(player, duration, true, _random.NextFloat(0.50f, 0.70f), _random.NextFloat(0.35f, 0.70f), statusComp);
            _status.TryAddStatusEffect<SeeingStaticComponent>(player, SeeingStaticSystem.StaticKey, duration, true, statusComp);
        }
    }


    private void OnPulse(EntityUid uid, OldTVAnomalyComponent component, ref TVAnomalyPulseEvent args)
    {
        var duration = component.MaxPulseStaticDuration;

        var xform = Transform(uid);
        var staticRadius = component.MaximumStaticRadius * args.Stability;
        StunNearby(uid, xform.Coordinates, duration, args.Severity, staticRadius);
    }

    private void OnSupercritical(EntityUid uid, OldTVAnomalyComponent component, ref TVAnomalySupercriticalEvent args)
    {
        var duration = component.MaxSupercritStaticDuration;

        var xform = Transform(uid);
        StunNearby(uid, xform.Coordinates, duration, 1, component.CritStaticRadius);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<OldTVAnomalyComponent, AnomalyComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var stat, out var anom, out var xform))
        {
            if (_timing.CurTime < stat.NextSecond)
                continue;
            stat.NextSecond = _timing.CurTime + TimeSpan.FromSeconds(0.5);

            if (!_random.Prob(stat.PassiveStaticChance * anom.Stability))
                continue;

            var range = stat.MaxStaticRange * anom.Stability * 4;
            var duration = stat.MaxPassiveStaticDuration * anom.Severity * 1;

            foreach (var (ent, comp) in _lookup.GetEntitiesInRange<StatusEffectsComponent>(xform.MapPosition, range))
            {
                _status.TryAddStatusEffect<AnomStaticComponent>(ent, AnomStaticSystem.StaticKey, duration, true, comp);
            }
            var closerange = stat.MaxStaticRange * anom.Stability * 2;
            var closeduration = stat.MaxPassiveStaticDuration * anom.Severity * 3;

            foreach (var (ent, comp) in _lookup.GetEntitiesInRange<StatusEffectsComponent>(xform.MapPosition, closerange))
            {
                _status.TryAddStatusEffect<AnomStaticComponent>(ent, AnomStaticSystem.StaticKey, closeduration, true, comp);
            }

        }
    }


}
