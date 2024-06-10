using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Alert;

namespace Content.Shared.GG.Drugs;

/// <summary>
///     Disable SlowOnDamage and DamageOverlay
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PainKillerComponent : Component {

    [DataField]
    public ProtoId<AlertPrototype> Alert = "PainKiller";
}
