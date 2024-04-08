using Content.Server.Actions;
using Content.Server.Store.Systems;
using Content.Shared.Phantom;
using Content.Shared.Phantom.Components;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Content.Shared.Movement.Events;
using Content.Server.Traitor.Uplink;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Utility;
using Content.Shared.IdentityManagement;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Alert;
using Robust.Server.GameObjects;
using Content.Shared.Tag;
using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Server.Stunnable;
using Content.Shared.Mind;
using Content.Shared.Humanoid;
using Robust.Shared.Containers;
using Content.Shared.DoAfter;
using System.Numerics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Network;

namespace Content.Server.Phantom.EntitySystems;

public sealed partial class PhantomSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] protected readonly SharedContainerSystem ContainerSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhantomComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PhantomComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<PhantomComponent, MakeHolderActionEvent>(OnMakeHolder);
        SubscribeLocalEvent<PhantomComponent, StopHauntingActionEvent>(OnStopHaunting);
        SubscribeLocalEvent<PhantomComponent, UpdateCanMoveEvent>(OnTryMove);

        SubscribeLocalEvent<PhantomComponent, CycleVesselActionEvent>(OnCycleVessel);
        SubscribeLocalEvent<PhantomComponent, HauntVesselActionEvent>(OnHauntVessel);
        SubscribeLocalEvent<PhantomComponent, MakeVesselActionEvent>(OnMakeVessel);
        SubscribeLocalEvent<PhantomComponent, MakeVesselDoAfterEvent>(MakeVesselDoAfter);
    }

    private void OnMapInit(EntityUid uid, PhantomComponent component, MapInitEvent args)
    {
        _action.AddAction(uid, ref component.PhantomHauntActionEntity, component.PhantomHauntAction);
        _action.AddAction(uid, ref component.PhantomMakeVesselActionEntity, component.PhantomMakeVesselAction);
        _action.AddAction(uid, ref component.PhantomCycleVesselsActionEntity, component.PhantomCycleVesselsAction);
        _action.AddAction(uid, ref component.PhantomHauntVesselActionEntity, component.PhantomHauntVesselAction);
    }

    private void OnShutdown(EntityUid uid, PhantomComponent component, ComponentShutdown args)
    {
    }

    private bool TryHaunt(EntityUid uid, EntityUid target)
    {
        if (HasComp<PhantomHolderComponent>(target))
        {
            var selfMessage = Loc.GetString("phantom-haunt-fail-self", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);
            return false;
        }

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            var selfMessageFailNoHuman = Loc.GetString("changeling-dna-fail-nohuman", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessageFailNoHuman, uid, uid);
            return false;
        }

        return true;
    }

    private void OnMakeHolder(EntityUid uid, PhantomComponent component, MakeHolderActionEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!component.HasHaunted)
            Haunt(uid, target, component);
        else
        {
            StopHaunt(uid, component.Holder, component);
            Haunt(uid, target, component);
        }

        args.Handled = true;

    }

    private void OnStopHaunting(EntityUid uid, PhantomComponent component, StopHauntingActionEvent args)
    {
        if (args.Handled)
            return;

        StopHaunt(uid, component.Holder, component);

        args.Handled = true;
    }

    private void OnTryMove(EntityUid uid, PhantomComponent component, UpdateCanMoveEvent args)
    {
        if (!component.HasHaunted)
            return;

        args.Cancel();
    }
    public void StopHaunt(EntityUid uid, EntityUid holder, PhantomComponent component)
    {
        if (!TryComp<PhantomHolderComponent>(holder, out var holderComp))
            return;
        if (!Resolve(holder, ref holderComp, false))
            return;

        if (!HasComp<PhantomComponent>(uid))
            return;

        RemComp<PhantomHolderComponent>(holder);
        component.Holder = new EntityUid();
        component.HasHaunted = false;
        _actionBlocker.UpdateCanMove(uid);

        _action.RemoveAction(uid, component.PhantomStopHauntActionEntity);

        var uidEv = new StoppedHauntEntityEvent(holder, uid);
        var targetEv = new EntityStoppedHauntEvent(holder, uid);

        RaiseLocalEvent(uid, uidEv, true);
        RaiseLocalEvent(holder, targetEv, false);
        Dirty(holder, holderComp);
        RaiseLocalEvent(uid, uidEv);
        RaiseLocalEvent(holder, targetEv);

        if (!TryComp(uid, out TransformComponent? xform))
            return;

        _transform.AttachToGridOrMap(uid, xform);
        if (xform.MapUid != null)
            return;

        if (_netMan.IsClient)
        {
            _transform.DetachParentToNull(uid, xform);
            return;
        }

    }

    public void Haunt(EntityUid uid, EntityUid target, PhantomComponent component)
    {
        if (TryHaunt(uid, target))
        {
            var targetXform = Transform(target);
            while (targetXform.ParentUid.IsValid())
            {
                if (targetXform.ParentUid == uid)
                    return;

                targetXform = Transform(targetXform.ParentUid);
            }

            if (component.Holder == target)
                return;

            component.HasHaunted = true;
            component.Holder = target;
            var holderComp = EnsureComp<PhantomHolderComponent>(target);
            _actionBlocker.UpdateCanMove(uid);

            var xform = Transform(uid);
            ContainerSystem.AttachParentToContainerOrGrid((uid, xform));

            // If we didn't get to parent's container.
            if (xform.ParentUid != Transform(xform.ParentUid).ParentUid)
            {
                _transform.SetCoordinates(uid, xform, new EntityCoordinates(target, Vector2.Zero), rotation: Angle.Zero);
            }

            _physicsSystem.SetLinearVelocity(uid, Vector2.Zero);

            var selfMessage = Loc.GetString("phantom-haunt-self", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);

            var targetMessage = Loc.GetString("phantom-haunt-target");
            _popup.PopupEntity(targetMessage, target, target);

            _action.AddAction(uid, ref component.PhantomStopHauntActionEntity, component.PhantomStopHauntAction);

            var followerEv = new StartedHauntEntityEvent(target, uid);
            var entityEv = new EntityStartedHauntEvent(target, uid);

            RaiseLocalEvent(uid, followerEv);
            RaiseLocalEvent(target, entityEv);
            Dirty(target, holderComp);

        }
    }

    public void OnCycleVessel(EntityUid uid, PhantomComponent component, CycleVesselActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (component.Vessels.Count < 1)
        {
            var selfMessage = Loc.GetString("phantom-no-vessels");
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        component.SelectedVessel += 1;

        if (component.Vessels.Count >= component.VesselsStrandCap || component.SelectedVessel >= component.Vessels.Count)
            component.SelectedVessel = 0;

        var selectedVessel = component.Vessels[component.SelectedVessel];

        TryComp<MetaDataComponent>(selectedVessel, out var meta);
        if (meta == null)
            return;

        var switchMessage = Loc.GetString("phantom-switch-vessel", ("target", meta.EntityName));
        _popup.PopupEntity(switchMessage, uid, uid);
    }

    private void OnHauntVessel(EntityUid uid, PhantomComponent component, HauntVesselActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (component.Vessels.Count < 1)
        {
            var selfMessage = Loc.GetString("phantom-no-vessels");
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        var target = component.Vessels[component.SelectedVessel];

        if (TryHaunt(uid, target))
        {
            if (!component.HasHaunted)
                Haunt(uid, target, component);
            else
            {
                StopHaunt(uid, component.Holder, component);
                Haunt(uid, target, component);
            }
        }
    }

    private void OnMakeVessel(EntityUid uid, PhantomComponent component, MakeVesselActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var target = args.Target;
        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            var selfMessage = Loc.GetString("phantom-vessel-fail-nohuman", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        args.Handled = true;
        var makeVesselDoAfter = new DoAfterArgs(EntityManager, uid, component.MakeVesselDuration, new MakeVesselDoAfterEvent(), uid, target: target)
        {
            DistanceThreshold = 15,
            BreakOnUserMove = true,
            BreakOnTargetMove = false,
            BreakOnDamage = true,
            AttemptFrequency = AttemptFrequency.StartAndEnd
        };
        _doAfter.TryStartDoAfter(makeVesselDoAfter);
    }

    private void MakeVesselDoAfter(EntityUid uid, PhantomComponent component, MakeVesselDoAfterEvent args)
    {
        if (args.Handled || args.Args.Target == null)
            return;

        args.Handled = true;

        var target = args.Args.Target.Value;
        if (args.Cancelled)
        {
            var selfMessage = Loc.GetString("phantom-vessel-interrupted");
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        var doMakeVessel = true;
        if (TryComp<HumanoidAppearanceComponent>(target, out _))
        {
            foreach (var storedVessels in component.Vessels)
            {
                if (storedVessels == target)
                    doMakeVessel = false;
            }
        }

        if (doMakeVessel)
        {
            if (!MakeVessel(target, component))
            {
                return;
            }
        }
    }

    public bool MakeVessel(EntityUid target, PhantomComponent component)
    {
        if (!TryComp<MetaDataComponent>(target, out _))
            return false;
        if (!TryComp<HumanoidAppearanceComponent>(target, out _))
            return false;
        if (component.Vessels.Count >= component.VesselsStrandCap)
            return false;
        else
            component.Vessels.Add(target);
        return true;
    }
}
