using Robust.Shared.Containers;
using Robust.Shared.GameStates;

namespace Content.Shared.Phantom.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class PhantomHolderComponent : Component
{
    [DataField]
    public EntityUid Phantom = new EntityUid();
}
