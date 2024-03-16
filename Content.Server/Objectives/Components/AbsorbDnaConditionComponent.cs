using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Requires that the player is on the emergency shuttle's grid when docking to CentCom.
/// </summary>
[RegisterComponent, Access(typeof(AbsorbDnaConditionSystem))]
public sealed partial class AbsorbDnaConditionComponent : Component
{
    [DataField]
    public float AbsorbDnaCount = 4f;

    [DataField]
    public float Zero = 0f;
}
