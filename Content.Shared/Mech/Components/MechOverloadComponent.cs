using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Shared.Mech.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MechOverloadComponent : Component
{
    [DataField] public EntityUid? MechOverloadActionEntity;
    public EntProtoId MechOverloadAction = "ActionMechOverload";
    public bool Overload = false;
    /// <summary>
    /// damage every x seconds if mech overloaded.
    /// </summary>

    [DataField("damage", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier DamagePerSpeed = default!;
    /// <summary>
    /// How much "health" the mech has left.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public FixedPoint2 MinIng;
    [DataField(required: true), AutoNetworkedField]
    public SoundSpecifier FootstepSoundOverload = default!;
    [DataField(required: true), AutoNetworkedField]
    public SoundSpecifier FootstepSoundNormal = default!;
}
