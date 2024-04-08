using Robust.Shared.GameStates;

namespace Content.Shared.Phantom.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class VesselComponent : Component
{
    [DataField]
    public EntityUid Phantom = new EntityUid();
}
