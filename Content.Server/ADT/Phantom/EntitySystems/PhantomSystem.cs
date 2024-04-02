using Content.Server.Actions;
using Content.Server.Store.Systems;
using Content.Shared.Phantom;
using Content.Shared.Phantom.Components;
using Content.Shared.Popups;
using Content.Shared.Store;
using Content.Server.Traitor.Uplink;
using Content.Shared.Mobs.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.IdentityManagement;
using Content.Server.Polymorph.Systems;
using Content.Server.Flash;
using Content.Shared.Actions;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Alert;
using Robust.Server.GameObjects;
using Content.Shared.Tag;
using Content.Shared.StatusEffect;
using Content.Shared.Movement.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Server.Stunnable;
using Content.Shared.Mind;
using Content.Shared.Humanoid;
using Robust.Shared.Containers;
using Content.Shared.DoAfter;
using Content.Shared.DoAfter;

namespace Content.Server.Phantom.EntitySystems;

public sealed partial class PhantomSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly UplinkSystem _uplink = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly FlashSystem _flashSystem = default!;
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

        SubscribeLocalEvent<PhantomComponent, CycleVesselActionEvent>(OnCycleVessel);
        SubscribeLocalEvent<PhantomComponent, HauntVesselActionEvent>(OnHauntVessel);
        SubscribeLocalEvent<PhantomComponent, MakeVesselActionEvent>(OnMakeVessel);
        SubscribeLocalEvent<PhantomComponent, MakeVesselDoAfterEvent>(MakeVesselDoAfter);
    }

    private void OnMapInit(EntityUid uid, PhantomComponent component, MapInitEvent args)
    {
        _action.AddAction(uid, ref component.PhantomHauntActionEntity, component.PhantomHauntAction);
    }

    private void OnShutdown(EntityUid uid, PhantomComponent component, ComponentShutdown args)
    {
    }

    private bool TryHaunt(EntityUid uid, EntityUid target, PhantomComponent component)
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

        if (TryHaunt(uid, target, component))
        {
            if (!component.HasHaunted)
            {
                args.Handled = true;

                var holder = EnsureComp<PhantomHolderComponent>(target);

                component.Holder = target;

                holder.Container = ContainerSystem.EnsureContainer<Container>(target, "soul");
                ContainerSystem.Insert(uid, holder.Container);

                component.HasHaunted = true;

                var selfMessage = Loc.GetString("phantom-haunt-self", ("target", Identity.Entity(target, EntityManager)));
                _popup.PopupEntity(selfMessage, uid, uid);

                var targetMessage = Loc.GetString("phantom-haunt-target");
                _popup.PopupEntity(targetMessage, target, target);
                _action.RemoveAction(uid, component.PhantomHauntActionEntity);
                _action.AddAction(uid, ref component.PhantomStopHauntActionEntity, component.PhantomStopHauntAction);
            }
        }
    }

    private void OnStopHaunting(EntityUid uid, PhantomComponent component, StopHauntingActionEvent args)
    {
        if (args.Handled)
            return;

        if (!component.HasHaunted)
            return;

        var haunted = EnsureComp<PhantomHolderComponent>(component.Holder);
        ContainerSystem.Remove(uid, haunted.Container);

        RemComp<PhantomHolderComponent>(component.Holder);
        component.HasHaunted = false;
        component.Holder = new EntityUid();
        _action.AddAction(uid, ref component.PhantomHauntActionEntity, component.PhantomHauntAction);
        _action.RemoveAction(uid, component.PhantomStopHauntActionEntity);
    }

    public void OnCycleVessel(EntityUid uid, PhantomComponent component, CycleVesselActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        component.SelectedVessel += 1;

        if (component.Vessels.Count >= component.VesselsStrandCap || component.SelectedVessel >= component.Vessels.Count)
            component.SelectedVessel = 0;

        var selectedVessel = component.Vessels[component.SelectedVessel];

        TryComp<MetaDataComponent>(selectedVessel, out var meta);
        if (meta == null)
            return;

        var selfMessage = Loc.GetString("phantom-switch-vessel", ("target", meta.EntityName));
        _popup.PopupEntity(selfMessage, uid, uid);
    }

    private void OnHauntVessel(EntityUid uid, PhantomComponent component, HauntVesselActionEvent args)
    {
        if (args.Handled)
            return;

        var target = component.Vessels[component.SelectedVessel];

        if (TryHaunt(uid, target, component))
        {
            if (!component.HasHaunted)
            {
                args.Handled = true;

                var holder = EnsureComp<PhantomHolderComponent>(target);

                component.Holder = target;

                holder.Container = ContainerSystem.EnsureContainer<Container>(target, "soul");
                ContainerSystem.Insert(uid, holder.Container);

                component.HasHaunted = true;

                var selfMessage = Loc.GetString("phantom-haunt-self", ("target", Identity.Entity(target, EntityManager)));
                _popup.PopupEntity(selfMessage, uid, uid);

                var targetMessage = Loc.GetString("phantom-haunt-target");
                _popup.PopupEntity(targetMessage, target, target);
                _action.RemoveAction(uid, component.PhantomHauntActionEntity);
                _action.AddAction(uid, ref component.PhantomStopHauntActionEntity, component.PhantomStopHauntAction);
            }
            else
            {
                args.Handled = true;

                var oldHolder = EnsureComp<PhantomHolderComponent>(component.Holder);
                ContainerSystem.Remove(uid, oldHolder.Container);
                RemComp<PhantomHolderComponent>(component.Holder);
                component.Holder = new EntityUid();

                var holder = EnsureComp<PhantomHolderComponent>(target);

                component.Holder = target;

                holder.Container = ContainerSystem.EnsureContainer<Container>(target, "soul");
                ContainerSystem.Insert(uid, holder.Container);

                component.HasHaunted = true;

                var selfMessage = Loc.GetString("phantom-haunt-self", ("target", Identity.Entity(target, EntityManager)));
                _popup.PopupEntity(selfMessage, uid, uid);

                var targetMessage = Loc.GetString("phantom-haunt-target");
                _popup.PopupEntity(targetMessage, target, target);
                _action.RemoveAction(uid, component.PhantomHauntActionEntity);
                _action.AddAction(uid, ref component.PhantomStopHauntActionEntity, component.PhantomStopHauntAction);
            }
        }
    }

    private void OnMakeVessel(EntityUid uid, PhantomComponent component, MakeVesselActionEvent args)
    {
        if (args.Handled)
            return;
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
            DistanceThreshold = 6,
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
            var selfMessage = Loc.GetString("phantom-vessel-interrupted", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        var doMakeVessel = true;
        if (TryComp<HumanoidAppearanceComponent>(target, out var trg))
        {
            foreach (var storedVessels in component.Vessels)
            {
                if (storedVessels != null && storedVessels == target)
                    doMakeVessel = false;
            }
        }

        if (doMakeVessel)
        {
            if (!MakeVessel(uid, target, component))
            {
                return;
            }
        }
    }

    public bool MakeVessel(EntityUid uid, EntityUid target, PhantomComponent component)
    {
        if (!TryComp<MetaDataComponent>(target, out var metaData))
            return false;
        if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoidappearance))
            return false;
        if (component.Vessels.Count >= component.VesselsStrandCap)
            return false;
        else
            component.Vessels.Add(target);
        return true;
    }
}
