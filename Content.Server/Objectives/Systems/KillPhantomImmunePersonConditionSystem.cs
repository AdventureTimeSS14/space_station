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
public sealed class KillPhantomImmunePersonConditionSystem : EntitySystem
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

        SubscribeLocalEvent<KillPhantomImmunePersonConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);

        SubscribeLocalEvent<PickPhantomImmunePersonComponent, ObjectiveAssignedEvent>(OnPersonAssigned);
    }

    private void OnGetProgress(EntityUid uid, KillPhantomImmunePersonConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        if (!_target.GetTarget(uid, out var target))
            return;

        args.Progress = GetProgress(target.Value, true);
    }

    private void OnPersonAssigned(EntityUid uid, PickPhantomImmunePersonComponent comp, ref ObjectiveAssignedEvent args)
    {
        // invalid objective prototype
        if (!TryComp<TargetObjectiveComponent>(uid, out var target))
        {
            args.Cancelled = true;
            return;
        }

        // target already assigned
        if (target.Target != null)
            return;

        // no other humans to kill
        var allHumans = _mind.GetAliveHumansExcept(args.MindId);
        if (allHumans.Count == 0)
        {
            args.Cancelled = true;
            return;
        }

        var pickedTarget = _random.Pick(allHumans);
        EnsureComp<PhantomImmuneComponent>(pickedTarget);
        _target.SetTarget(uid, pickedTarget, target);

    }

    private float GetProgress(EntityUid target, bool requireDead)
    {
        // deleted or gibbed or something, counts as dead
        if (!TryComp<MindComponent>(target, out var mind) || mind.OwnedEntity == null)
            return 1f;

        // dead is success
        if (_mind.IsCharacterDeadIc(mind))
            return 1f;

        // if the target has to be dead dead then don't check evac stuff
        if (requireDead)
            return 0f;

        // if evac is disabled then they really do have to be dead
        if (!_config.GetCVar(CCVars.EmergencyShuttleEnabled))
            return 0f;

        // target is escaping so you fail
        if (_emergencyShuttle.IsTargetEscaping(mind.OwnedEntity.Value))
            return 0f;

        // evac has left without the target, greentext since the target is afk in space with a full oxygen tank and coordinates off.
        if (_emergencyShuttle.ShuttlesLeft)
            return 1f;

        // if evac is still here and target hasn't boarded, show 50% to give you an indicator that you are doing good
        return _emergencyShuttle.EmergencyShuttleArrived ? 0.5f : 0f;
    }
}
