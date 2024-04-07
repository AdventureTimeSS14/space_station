using Content.Server.Gateway.Components;
using Content.Server.Gateway.Systems;

namespace Content.Server.ADT.ActiveGateway;

[RegisterComponent, Access(typeof(ActiveGatewaySystem))]
public sealed partial class ActiveGatewayComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool Enabled = true;
}
