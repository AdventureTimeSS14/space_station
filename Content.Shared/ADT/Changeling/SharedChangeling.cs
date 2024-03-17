using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Changeling;

public sealed partial class LingAbsorbActionEvent : EntityTargetActionEvent
{
}

public sealed partial class LingStingExtractActionEvent : EntityTargetActionEvent
{
}

public sealed partial class BlindStingEvent : EntityTargetActionEvent
{
}

public sealed partial class MuteStingEvent : EntityTargetActionEvent
{
}

public sealed partial class DrugStingEvent : EntityTargetActionEvent
{
}
public sealed partial class LingEggActionEvent : EntityTargetActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class AbsorbDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class LingEggDoAfterEvent : SimpleDoAfterEvent
{
}

public sealed partial class ChangelingEvolutionMenuActionEvent : InstantActionEvent
{
}

public sealed partial class ChangelingCycleDNAActionEvent : InstantActionEvent
{
}

public sealed partial class ChangelingTransformActionEvent : InstantActionEvent
{
}

public sealed partial class LingRegenerateActionEvent : InstantActionEvent
{
}

public sealed partial class ArmBladeActionEvent : InstantActionEvent
{
}

public sealed partial class LingArmorActionEvent : InstantActionEvent
{
}

public sealed partial class LingInvisibleActionEvent : InstantActionEvent
{
}

public sealed partial class LingEMPActionEvent : InstantActionEvent
{
}

public sealed partial class StasisDeathActionEvent : InstantActionEvent
{
}

public sealed partial class AdrenalineActionEvent : InstantActionEvent
{
}

public sealed partial class OmniHealActionEvent : InstantActionEvent
{
}

public sealed partial class ChangelingRefreshActionEvent : InstantActionEvent
{
}

public sealed partial class ChangelingMusclesActionEvent : InstantActionEvent
{
}

public sealed partial class ChangelingLesserFormActionEvent : InstantActionEvent
{
}

public sealed partial class ArmShieldActionEvent : InstantActionEvent
{
}

public sealed partial class LastResortActionEvent : InstantActionEvent
{
}

public sealed partial class LingEggSpawnActionEvent : InstantActionEvent
{
}

public sealed partial class LingHatchActionEvent : InstantActionEvent
{
}
