using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Phantom;

public sealed partial class MakeHolderActionEvent : EntityTargetActionEvent
{
}

public sealed partial class MakeVesselActionEvent : EntityTargetActionEvent
{
}

public sealed partial class ParalysisActionEvent : EntityTargetActionEvent
{
}

public sealed partial class StopHauntingActionEvent : InstantActionEvent
{
}

public sealed partial class CycleVesselActionEvent : InstantActionEvent
{
}

public sealed partial class HauntVesselActionEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class MakeVesselDoAfterEvent : SimpleDoAfterEvent
{
}



public abstract class HauntEvent : EntityEventArgs
{
    public EntityUid Following;
    public EntityUid Follower;

    protected HauntEvent(EntityUid following, EntityUid follower)
    {
        Following = following;
        Follower = follower;
    }
}

public sealed class StartedHauntEntityEvent : HauntEvent
{
    public StartedHauntEntityEvent(EntityUid following, EntityUid follower) : base(following, follower)
    {
    }
}

public sealed class StoppedHauntEntityEvent : HauntEvent
{
    public StoppedHauntEntityEvent(EntityUid following, EntityUid follower) : base(following, follower)
    {
    }
}

public sealed class EntityStartedHauntEvent : HauntEvent
{
    public EntityStartedHauntEvent(EntityUid following, EntityUid follower) : base(following, follower)
    {
    }
}

public sealed class EntityStoppedHauntEvent : HauntEvent
{
    public EntityStoppedHauntEvent(EntityUid following, EntityUid follower) : base(following, follower)
    {
    }
}

[NetSerializable, Serializable]
public enum PhantomVisuals : byte
{
    Corporeal,
    Haunting,
}

public sealed class PhantomSpeechGetEvent : EntityEventArgs
{
    public EntityUid Entity { get; }

    public string Message { get; set; }

    public PhantomSpeechGetEvent(EntityUid entity, string message)
    {
        Entity = entity;
        Message = message;
    }
}
