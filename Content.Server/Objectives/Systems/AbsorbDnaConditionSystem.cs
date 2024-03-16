using Content.Server.Objectives.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Changeling.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed class AbsorbDnaConditionSystem : EntitySystem
{
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbsorbDnaConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, AbsorbDnaConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = GetProgress(args.MindId, args.Mind, comp);
    }

    private float GetProgress(EntityUid mindId, MindComponent mind, AbsorbDnaConditionComponent comp)
    {
        // Не генокрад - не выполнил.
        if (!TryComp<ChangelingComponent>(mindId, out var ling))
            return 0f;

        // Умер - не смог вернуться с днк
        if (mind.OwnedEntity == null || _mind.IsCharacterDeadIc(mind))
            return 0f;

        // Ниже идут сравнения поглощённых и требуемых
        if (ling.AbsorbedDnaModifier == comp.Zero)
            return 0f;

        if (ling.AbsorbedDnaModifier > comp.Zero && ling.AbsorbedDnaModifier < comp.AbsorbDnaCount / 2)
            return 0.25f;

        if (ling.AbsorbedDnaModifier == comp.AbsorbDnaCount / 2)
            return 0.5f;

        if (ling.AbsorbedDnaModifier > comp.Zero && ling.AbsorbedDnaModifier > comp.AbsorbDnaCount / 2 && ling.AbsorbedDnaModifier < comp.AbsorbDnaCount)
            return 0.75f;

        if (ling.AbsorbedDnaModifier >= comp.AbsorbDnaCount)
            return 1f;

        return 0f;
    }
}
