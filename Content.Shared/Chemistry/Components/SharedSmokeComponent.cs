using Robust.Shared.GameStates;

namespace Content.Shared.Chemistry.Components;

[NetworkedComponent]
public partial class SharedSmokeComponent : Component
{
    [DataField("color")]
    public Color Color = Color.White;
}
