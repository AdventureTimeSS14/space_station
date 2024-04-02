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
public sealed partial class StopHauntingActionEvent : InstantActionEvent
{
}

public sealed partial class CycleVesselActionEvent : InstantActionEvent
{
}

public sealed partial class HauntVesselActionEvent : InstantActionEvent
{
}

public sealed partial class MakeVesselDoAfterEvent : SimpleDoAfterEvent
{
}
