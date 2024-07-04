using Content.Shared.Phantom.Components;
using Content.Shared.Phantom;
using Content.Shared.Mobs;
using Content.Shared.Eye;

namespace Content.Server.Phantom.EntitySystems;

public sealed partial class PhantomVesselSystem : EntitySystem
{
    [Dependency] private readonly PhantomSystem _phantom = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VesselComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<VesselComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<VesselComponent, MobStateChangedEvent>(OnDeath);
        SubscribeLocalEvent<VesselComponent, EntityTerminatingEvent>(OnDeleted);
        SubscribeLocalEvent<PhantomHolderComponent, MapInitEvent>(OnHauntedInit);
        SubscribeLocalEvent<PhantomHolderComponent, MobStateChangedEvent>(OnHauntedDeath);
        SubscribeLocalEvent<PhantomHolderComponent, ComponentShutdown>(OnHauntedShutdown);
        SubscribeLocalEvent<PhantomHolderComponent, EntityTerminatingEvent>(OnHauntedDeleted);
    }
    private void OnMapInit(EntityUid uid, VesselComponent component, MapInitEvent args)
    {
        if (!TryComp<EyeComponent>(uid, out var eyeComponent))
            return;
        _eye.SetVisibilityMask(uid, eyeComponent.VisibilityMask | (int) VisibilityFlags.PhantomVessel, eyeComponent);
    }
    private void OnShutdown(EntityUid uid, VesselComponent component, ComponentShutdown args)
    {
        if (!TryComp<PhantomComponent>(component.Phantom, out var phantom))
            return;

        phantom.Vessels.Remove(uid);
        var ev = new RefreshPhantomLevelEvent();
        RaiseLocalEvent(component.Phantom, ev);

        if (phantom.Holder == uid)
            _phantom.StopHaunt(component.Phantom, uid, phantom);

        if (!TryComp<EyeComponent>(uid, out var eyeComponent))
            return;
        _eye.SetVisibilityMask(uid, eyeComponent.VisibilityMask & ~(int) VisibilityFlags.PhantomVessel, eyeComponent);
    }

    private void OnDeath(EntityUid uid, VesselComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            TryComp<PhantomComponent>(component.Phantom, out var comp);
            RemComp<VesselComponent>(uid);
            if (comp != null)
            {
                if (comp.TyranyStarted && comp.Vessels.Count <= 0)
                    _phantom.ChangeEssenceAmount(component.Phantom, -1000, allowDeath: true);
            }
        }
    }

    private void OnHauntedDeath(EntityUid uid, PhantomHolderComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            if (TryComp<PhantomComponent>(component.Phantom, out var comp))
            {
                if (comp.TyranyStarted && comp.Vessels.Count <= 0)
                    _phantom.ChangeEssenceAmount(component.Phantom, -1000, allowDeath: true);
            }
        }
    }

    private void OnHauntedInit(EntityUid uid, PhantomHolderComponent component, MapInitEvent args)
    {
        EnsureComp<PhantomHolderIconComponent>(uid);
    }

    private void OnHauntedShutdown(EntityUid uid, PhantomHolderComponent component, ComponentShutdown args)
    {
        if (HasComp<PhantomHolderIconComponent>(uid))
            RemComp<PhantomHolderIconComponent>(uid);
    }

    private void OnHauntedDeleted(EntityUid uid, PhantomHolderComponent component, EntityTerminatingEvent args)
    {
        if (!TryComp<PhantomComponent>(component.Phantom, out var phantom))
            return;
        _phantom.StopHaunt(component.Phantom, uid, phantom);
    }

    private void OnDeleted(EntityUid uid, VesselComponent component, EntityTerminatingEvent args)
    {
        if (!TryComp<PhantomComponent>(component.Phantom, out var phantom))
            return;

        phantom.Vessels.Remove(uid);
        var ev = new RefreshPhantomLevelEvent();
        RaiseLocalEvent(component.Phantom, ev);

        if (phantom.Holder == uid)
            _phantom.StopHaunt(component.Phantom, uid, phantom);

        if (!TryComp<EyeComponent>(uid, out var eyeComponent))
            return;
        _eye.SetVisibilityMask(uid, eyeComponent.VisibilityMask & ~(int) VisibilityFlags.PhantomVessel, eyeComponent);
    }
}
