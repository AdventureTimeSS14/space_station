using Content.Shared.Phantom.Components;
using Content.Server.Phantom.EntitySystems;

namespace Content.Server.Phantom.EntitySystems;

public sealed partial class PhantomVesselSystem : EntitySystem
{
    [Dependency] private readonly PhantomSystem _phantom = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VesselComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnShutdown(EntityUid uid, VesselComponent component, ComponentShutdown args)
    {
        if (!TryComp<PhantomComponent>(component.Phantom, out var phantom))
            return;
        if (phantom.Holder == uid)
        {
            phantom.Vessels.Remove(uid);
            _phantom.StopHaunt(component.Phantom, uid, phantom);

            if (phantom.Vessels[phantom.SelectedVessel] == uid)
            {
                _phantom.CycleVessel(component.Phantom, phantom);
            }
        }

    }

}
