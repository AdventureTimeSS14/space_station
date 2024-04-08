using Content.Server.Gateway.Components;
using Content.Server.Gateway.Systems;

namespace Content.Server.ADT.ActiveGateway;

public sealed class ActiveGatewaySystem : EntitySystem
{
    [Dependency] private readonly GatewaySystem _gatewaySystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ActiveGatewayComponent, MapInitEvent>(OnComponentStartup);
    }

    private void OnComponentStartup(EntityUid uid, ActiveGatewayComponent component, MapInitEvent args)
    {
        if(!TryComp<GatewayComponent>(uid, out var gatewayComponent))
            return;

        OnStartGateway(uid, component.Enabled, component);
    }

    private void OnStartGateway(EntityUid uid, bool value, ActiveGatewayComponent? component = null)
    {
        if (!Resolve(uid, ref component) || component.Enabled == value)
            return;

        _gatewaySystem.SetEnabled(uid, component.Enabled);
    }
}
