using Content.Server.Objectives.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.CCVar;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Roles.Jobs;
using Robust.Shared.Configuration;
using Robust.Shared.Random;
using Content.Shared.Phantom.Components;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// Handles kill person condition logic and picking random kill targets.
/// </summary>
public sealed class PhantomReincarnateConditionSystem : EntitySystem
{
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedJobSystem _job = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhantomReincarnateConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);

    }

    private void OnGetProgress(EntityUid uid, PhantomReincarnateConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(args.Mind.OwnedEntity);
    }

    private float GetProgress(EntityUid? uid)
    {
        if (uid == null)
            return 0f;
        if (TryComp<PhantomComponent>(uid, out var component))
        {
            if (!component.FinalAbilityUsed)
                return (float) Math.Clamp(component.Vessels.Count, 0f, 0.9f);
            else
                return 0f;
        }
        if (!HasComp<PhantomPuppetComponent>(uid))
            return 0f;
        return 1f;
    }
}
