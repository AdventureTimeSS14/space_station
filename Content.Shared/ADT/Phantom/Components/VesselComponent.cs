using Robust.Shared.GameStates;
using Content.Shared.StatusIcon;
using Content.Shared.Antag;
using Robust.Shared.Prototypes;

namespace Content.Shared.Phantom.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VesselComponent : Component, IAntagStatusIconComponent
{
    [DataField]
    public EntityUid Phantom = new EntityUid();

    [DataField("vesselStatusIcon")]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "PhantomVesselFaction";

    [DataField]
    public bool IconVisibleToGhost { get; set; } = true;
}
