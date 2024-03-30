using Robust.Shared.Containers;

namespace Content.Shared.Phantom.Components;

[RegisterComponent]
public sealed partial class PhantomHolderComponent : Component
{
    public Container Container = default!;
}
