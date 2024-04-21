using Content.Shared.Phantom.Components;
using Content.Server.Phantom.EntitySystems;
using Content.Shared.Phantom;

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
        var oldLV = phantom.Vessels.Count;
        phantom.Vessels.Remove(uid);

        var vessel = phantom.Vessels[phantom.SelectedVessel];

        if (vessel == uid)
            _phantom.CycleVessel(component.Phantom, phantom);

        if (phantom.Holder == uid)
            _phantom.StopHaunt(component.Phantom, uid, phantom);

        var lv = phantom.Vessels.Count;

        var ev = new RefreshPhantomLevelEvent(oldLV, lv);
        RaiseLocalEvent(component.Phantom, ev);
    }

}
