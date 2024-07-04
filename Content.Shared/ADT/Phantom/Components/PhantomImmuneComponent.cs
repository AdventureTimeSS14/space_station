using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;
using Content.Shared.Antag;
using Robust.Shared.Prototypes;

namespace Content.Shared.Phantom.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PhantomImmuneComponent : Component, IAntagStatusIconComponent
{
    [DataField("statusIcon")]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "PhantomImmune";

    [DataField]
    public bool IconVisibleToGhost { get; set; } = true;
}
