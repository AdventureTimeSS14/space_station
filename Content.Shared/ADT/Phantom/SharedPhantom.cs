using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Robust.Shared.Serialization;
using Content.Shared.Preferences;

namespace Content.Shared.Phantom;

#region EntityTarget Actions
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

public sealed partial class StarvationActionEvent : EntityTargetActionEvent
{
}

public sealed partial class RepairActionEvent : EntityTargetActionEvent
{
}

public sealed partial class BloodBlindingActionEvent : EntityTargetActionEvent
{
}

public sealed partial class PsychoEpidemicActionEvent : EntityTargetActionEvent
{
}
#endregion

#region Instant Actions
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

public sealed partial class OpenPhantomStylesMenuActionEvent : InstantActionEvent
{
}

public sealed partial class ShieldBreakActionEvent : InstantActionEvent
{
}

public sealed partial class GhostClawsActionEvent : InstantActionEvent
{
}

public sealed partial class GhostInjuryActionEvent : InstantActionEvent
{
}

public sealed partial class GhostHealActionEvent : InstantActionEvent
{
}

public sealed partial class PuppeterActionEvent : InstantActionEvent
{
}

public sealed partial class PhantomPortalActionEvent : InstantActionEvent
{
}

public sealed partial class PhantomHelpingHelpActionEvent : InstantActionEvent
{
}

public sealed partial class PhantomControlActionEvent : InstantActionEvent
{
}
#endregion

#region Finale
public sealed partial class NightmareFinaleActionEvent : InstantActionEvent
{
}

public sealed partial class TyranyFinaleActionEvent : InstantActionEvent
{
}

public sealed partial class FreedomFinaleActionEvent : InstantActionEvent
{
}

public sealed partial class FreedomOblivionFinaleActionEvent : InstantActionEvent
{
}

public sealed partial class FreedomDeathmatchFinaleActionEvent : InstantActionEvent
{
}

public sealed partial class FreedomHelpFinaleActionEvent : InstantActionEvent
{
}

public enum PhantomFinaleType : byte
{
    Nightmare = 0,
    Tyrany = 1,
    Oblivion = 2,
    Deathmatch = 3,
    Help = 4
}
#endregion

#region DoAfter's
[Serializable, NetSerializable]
public sealed partial class MakeVesselDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class PuppeterDoAfterEvent : SimpleDoAfterEvent
{
}
#endregion

#region Events
public sealed class RefreshPhantomLevelEvent : EntityEventArgs
{
    public RefreshPhantomLevelEvent()
    {
    }
}

public sealed class PhantomReincarnatedEvent : EntityEventArgs
{
    public PhantomReincarnatedEvent()
    {
    }
}

public sealed class PhantomDiedEvent : EntityEventArgs
{
    public PhantomDiedEvent()
    {
    }
}

public sealed class PhantomTyranyEvent : EntityEventArgs
{
    public PhantomTyranyEvent()
    {
    }
}

public sealed class PhantomNightmareEvent : EntityEventArgs
{
    public PhantomNightmareEvent()
    {
    }
}
#endregion

#region Radial Menu
/// <summary>
/// This event carries list of style prototypes and entity - the source of request. This class is a part of code which is responsible for using RadialUiController.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class RequestPhantomStyleMenuEvent : EntityEventArgs
{
    public readonly List<string> Prototypes = new();
    public NetEntity Target;

    public RequestPhantomStyleMenuEvent(NetEntity target)
    {
        Target = target;
    }
}

/// <summary>
/// This event carries prototype-id of style, which was selected. This class is a part of code which is responsible for using RadialUiController.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SelectPhantomStyleEvent : EntityEventArgs
{
    public string PrototypeId;
    public NetEntity Target;
    public bool Handled = false;

    public SelectPhantomStyleEvent(NetEntity target, string prototypeId)
    {
        Target = target;
        PrototypeId = prototypeId;
    }
}

[Serializable, NetSerializable]
public sealed partial class RequestPhantomFreedomMenuEvent : EntityEventArgs
{
    public readonly List<string> Prototypes = new();
    public NetEntity Target;

    public RequestPhantomFreedomMenuEvent(NetEntity target)
    {
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed partial class SelectPhantomFreedomEvent : EntityEventArgs
{
    public string PrototypeId;
    public NetEntity Target;
    public bool Handled = false;

    public SelectPhantomFreedomEvent(NetEntity target, string prototypeId)
    {
        Target = target;
        PrototypeId = prototypeId;
    }
}

[Serializable, NetSerializable]
public sealed partial class RequestPhantomVesselMenuEvent : EntityEventArgs
{
    public NetEntity Uid;
    public readonly List<(NetEntity, HumanoidCharacterProfile, string)> Vessels = new();

    public RequestPhantomVesselMenuEvent(NetEntity uid, List<(NetEntity, HumanoidCharacterProfile, string)> vessels)
    {
        Uid = uid;
        Vessels = vessels;
    }
}

[Serializable, NetSerializable]
public sealed partial class SelectPhantomVesselEvent : EntityEventArgs
{
    public NetEntity Uid;
    public NetEntity Vessel;

    public SelectPhantomVesselEvent(NetEntity uid, NetEntity vessel)
    {
        Uid = uid;
        Vessel = vessel;
    }
}
#endregion

#region Visualizer
[NetSerializable, Serializable]
public enum PhantomVisuals : byte
{
    Corporeal,
    Stunned,
    Haunting,
}
#endregion

#region Puppets
public sealed partial class SelfGhostClawsActionEvent : InstantActionEvent
{
}

public sealed partial class SelfGhostHealActionEvent : InstantActionEvent
{
}
#endregion
