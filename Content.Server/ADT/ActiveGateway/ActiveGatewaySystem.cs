using Content.Server.Gateway.Components;
using Content.Server.Gateway.Systems;

namespace Content.Server.ADT.ActiveGateway;

public sealed class ActiveGatewaySystem : EntitySystem
{
    [Dependency] private readonly GatewaySystem _gatewaySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ActiveGatewayComponent, ComponentStartup>(OnComponentStartup);
    }

    private void OnComponentStartup(EntityUid uid, ActiveGatewayComponent component, ComponentStartup args)
    {
        _gatewaySystem.SetEnabled(uid, component.Enabled);
    }
}
