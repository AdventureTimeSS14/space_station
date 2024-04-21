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

public sealed partial class BreakdownActionEvent : EntityTargetActionEvent
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

public sealed partial class MaterializeActionEvent : InstantActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class MakeVesselDoAfterEvent : SimpleDoAfterEvent
{
}

public sealed class RefreshPhantomLevelEvent : EntityEventArgs
{
    public int PrevLV;
    public int NewLV;

    public RefreshPhantomLevelEvent(int prevLV, int newLV)
    {
        PrevLV = prevLV;
        NewLV = newLV;
    }
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

