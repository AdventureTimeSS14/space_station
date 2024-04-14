using Content.Server.Actions;
using Content.Shared.Physics;
using Robust.Shared.Physics;
using Content.Shared.Phantom;
using Content.Shared.Phantom.Components;
using Content.Shared.Popups;
using Robust.Shared.Map;
using Content.Shared.Movement.Events;
using Content.Shared.Rejuvenate;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Random;
using Content.Shared.IdentityManagement;
using Content.Shared.Chat;
using Content.Shared.Actions;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Alert;
using Robust.Server.GameObjects;
using Content.Server.Chat.Systems;
using Content.Shared.ActionBlocker;
using Content.Shared.StatusEffect;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mind;
using Content.Shared.Humanoid;
using Robust.Shared.Containers;
using Content.Shared.DoAfter;
using System.Numerics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Network;
using Content.Shared.Mobs;
using Content.Server.Chat.Managers;
using Robust.Shared.Prototypes;
using System.Linq;
using Content.Shared.Stunnable;
using Robust.Shared.Player;
using Content.Shared.Eye;

namespace Content.Server.Phantom.EntitySystems;

public sealed partial class PhantomSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly ISerializationManager _serialization = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly VisibilitySystem _visibility = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly SharedPhysicsSystem _physicsSystem = default!;
    [Dependency] protected readonly SharedContainerSystem ContainerSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Startup
        SubscribeLocalEvent<PhantomComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PhantomComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PhantomComponent, ComponentStartup>(OnStartup);

        // Haunting
        SubscribeLocalEvent<PhantomComponent, MakeHolderActionEvent>(OnMakeHolder);
        SubscribeLocalEvent<PhantomComponent, StopHauntingActionEvent>(OnStopHaunting);
        SubscribeLocalEvent<PhantomComponent, UpdateCanMoveEvent>(OnTryMove);

        // Vessels manipulations
        SubscribeLocalEvent<PhantomComponent, CycleVesselActionEvent>(OnCycleVessel);
        SubscribeLocalEvent<PhantomComponent, HauntVesselActionEvent>(OnHauntVessel);
        SubscribeLocalEvent<PhantomComponent, MakeVesselActionEvent>(OnMakeVessel);
        SubscribeLocalEvent<PhantomComponent, MakeVesselDoAfterEvent>(MakeVesselDoAfter);

        // Styles
        SubscribeLocalEvent<PhantomComponent, SelectPhantomStyleActionEvent>(OnStyleAction);
        SubscribeNetworkEvent<SelectPhantomStyleEvent>(OnSelectStyle);

        // Abilities
        SubscribeLocalEvent<PhantomComponent, ParalysisActionEvent>(OnParalysis);
        SubscribeLocalEvent<PhantomComponent, MaterializeActionEvent>(OnCorporeal);

        // Other
        SubscribeLocalEvent<PhantomComponent, AlternativeSpeechEvent>(OnTrySpeak);
        SubscribeLocalEvent<PhantomComponent, DamageChangedEvent>(OnDamage);
        SubscribeLocalEvent<PhantomComponent, RefreshPhantomLevelEvent>(OnLevelChanged);
    }

    private void OnStartup(EntityUid uid, PhantomComponent component, ComponentStartup args)
    {
        _appearance.SetData(uid, PhantomVisuals.Haunting, false);
        ChangeEssenceAmount(uid, 0, component);
    }

    private void OnMapInit(EntityUid uid, PhantomComponent component, MapInitEvent args)
    {
        _action.AddAction(uid, ref component.PhantomHauntActionEntity, component.PhantomHauntAction);
        _action.AddAction(uid, ref component.PhantomMakeVesselActionEntity, component.PhantomMakeVesselAction);
        _action.AddAction(uid, ref component.PhantomCycleVesselsActionEntity, component.PhantomCycleVesselsAction);
    }

    private void OnShutdown(EntityUid uid, PhantomComponent component, ComponentShutdown args)
    {
    }

    public bool ChangeEssenceAmount(EntityUid uid, FixedPoint2 amount, PhantomComponent? component = null, bool allowDeath = true, bool regenCap = false)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (!allowDeath && component.Essence + amount <= 0)
            return false;

        component.Essence += amount;

        if (regenCap)
            FixedPoint2.Min(component.Essence, component.EssenceRegenCap);

        _alerts.ShowAlert(uid, AlertType.Essence, (short) Math.Clamp(Math.Round(component.Essence.Float() / 10f), 0, 16));

        if (component.Essence <= 0)
        {
            if (!TryRevive(uid, component))
            {
                Spawn(component.SpawnOnDeathPrototype, Transform(uid).Coordinates);
                QueueDel(uid);
            }
        }
        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PhantomComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            comp.Accumulator += frameTime;

            if (comp.Accumulator <= 1)
                continue;
            comp.Accumulator -= 1;

            if (comp.Essence < comp.EssenceRegenCap)
            {
                ChangeEssenceAmount(uid, comp.EssencePerSecond, comp, regenCap: true);
            }
        }
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

    private void OnLevelChanged(EntityUid uid, PhantomComponent component, RefreshPhantomLevelEvent args)
    {
        if (args.NewLV == args.PrevLV || args.NewLV == null || args.PrevLV == null)
            return;

        var level = component.Vessels.Count;

        if (args.NewLV > args.PrevLV)
        {
            if (level == 1)
            {
                _action.AddAction(uid, ref component.PhantomHauntVesselActionEntity, component.PhantomHauntVesselAction);
            }
            if (level == 2)
            {
                _action.AddAction(uid, ref component.PhantomParalysisActionEntity, component.PhantomParalysisAction);
            }
        }

        if (args.NewLV < args.PrevLV)
        {
            if (level == 0)
            {
                _action.RemoveAction(uid, component.PhantomHauntVesselActionEntity);
            }
            if (level == 1)
            {
                _action.RemoveAction(uid, component.PhantomParalysisActionEntity);
            }
        }
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
        HauntedStopEffects(component.Holder, component);
        component.Holder = new EntityUid();
        component.HasHaunted = false;
        _actionBlocker.UpdateCanMove(uid);

        _action.RemoveAction(uid, component.PhantomStopHauntActionEntity);

        var eye = EnsureComp<EyeComponent>(uid);
        _eye.SetDrawFov(uid, false, eye);
        _appearance.SetData(uid, PhantomVisuals.Haunting, false);

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
            holderComp.Phantom = uid;
            _actionBlocker.UpdateCanMove(uid);

            var eye = EnsureComp<EyeComponent>(uid);
            _eye.SetDrawFov(uid, true, eye);
            _appearance.SetData(uid, PhantomVisuals.Haunting, true);
            EnsureComp<AlternativeSpeechComponent>(uid);

            var xform = Transform(uid);
            ContainerSystem.AttachParentToContainerOrGrid((uid, xform));

            // If we didn't get to parent's container.
            if (xform.ParentUid != Transform(xform.ParentUid).ParentUid)
            {
                _transform.SetCoordinates(uid, xform, new EntityCoordinates(target, Vector2.Zero), rotation: Angle.Zero);
            }

            if (component.IsCorporeal)
            {
                if (TryComp<FixturesComponent>(uid, out var fixtures) && fixtures.FixtureCount >= 1)
                {
                    var fixture = fixtures.Fixtures.First();

                    _physicsSystem.SetCollisionMask(uid, fixture.Key, fixture.Value, (int) CollisionGroup.GhostImpassable, fixtures);
                    _physicsSystem.SetCollisionLayer(uid, fixture.Key, fixture.Value, 0, fixtures);
                }
                var visibility = EnsureComp<VisibilityComponent>(uid);

                _visibility.SetLayer(uid, visibility, (int) VisibilityFlags.Ghost, false);
                _visibility.RefreshVisibility(uid);
                component.IsCorporeal = false;
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

    public void CycleVessel(EntityUid uid, PhantomComponent component)
    {
        if (component.Vessels.Count < 1)
        {
            component.SelectedVessel = 0;
            return;
        }

        component.SelectedVessel += 1;

        if (component.Vessels.Count >= component.VesselsStrandCap || component.SelectedVessel >= component.Vessels.Count)
            component.SelectedVessel = 0;
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

        CycleVessel(uid, component);

        var selectedVessel = component.Vessels[component.SelectedVessel];

        if (!TryComp<MetaDataComponent>(selectedVessel, out var meta))
            return;

        var switchMessage = Loc.GetString("phantom-switch-vessel", ("vessel", Identity.Entity(selectedVessel, EntityManager)));
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
            var selfMessage = Loc.GetString("changeling-dna-fail-nohuman", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        if (HasComp<VesselComponent>(target))
        {
            var selfMessage = Loc.GetString("phantom-vessel-fail-already", ("target", Identity.Entity(target, EntityManager)));
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
            if (!MakeVessel(uid, target, component))
            {
                return;
            }
        }
    }

    public bool MakeVessel(EntityUid uid, EntityUid target, PhantomComponent component)
    {
        if (!TryComp<MetaDataComponent>(target, out _))
            return false;
        if (!TryComp<HumanoidAppearanceComponent>(target, out _))
            return false;
        if (component.Vessels.Count >= component.VesselsStrandCap)
            return false;
        else
        {
            var oldLV = component.Vessels.Count;
            component.Vessels.Add(target);
            var vessel = EnsureComp<VesselComponent>(target);
            vessel.Phantom = uid;
            var lv = component.Vessels.Count;

            var ev = new RefreshPhantomLevelEvent(oldLV, lv);
            RaiseLocalEvent(uid, ev);
        }
        return true;
    }

    public bool TryRevive(EntityUid uid, PhantomComponent component)
    {
        if (component.Vessels.Count < 1)
            return false;

        var randomVessel = _random.Pick(component.Vessels);
        ChangeEssenceAmount(uid, component.Essence, component);

        if (!component.HasHaunted)
        {
            Haunt(uid, randomVessel, component);
        }
        else
        {
            StopHaunt(uid, component.Holder, component);
            Haunt(uid, randomVessel, component);
        }
        return true;
    }

    public void OnTrySpeak(EntityUid uid, PhantomComponent component, AlternativeSpeechEvent args)
    {
        if (!component.HasHaunted)
        {
                var selfMessage = Loc.GetString("phantom-say-fail");
                _popup.PopupEntity(selfMessage, uid, uid);
                return;
        }

        else
        {
            var target = component.Holder;
            var popupMessage = Loc.GetString("phantom-say-target");
            var selfMessage = Loc.GetString("phantom-say-self");

            if (!_mindSystem.TryGetMind(target, out var mindId, out var mind) || mind.Session == null)
                return;
            if (!_mindSystem.TryGetMind(uid, out var selfMindId, out var selfMind) || selfMind.Session == null)
                return;

            _popup.PopupEntity(popupMessage, target, target, PopupType.MediumCaution);
            _popup.PopupEntity(selfMessage, uid, uid);

            var message = popupMessage == "" ? "" : popupMessage + (args.Message == "" ? "" : $" \"{args.Message}\"");
            _chatManager.ChatMessageToOne(ChatChannel.Local, args.Message, message, EntityUid.Invalid, false, mind.Session.Channel);

            var selfChatMessage = selfMessage == "" ? "" : selfMessage + (args.Message == "" ? "" : $" \"{args.Message}\"");
            _chatManager.ChatMessageToOne(ChatChannel.Local, args.Message, selfChatMessage, EntityUid.Invalid, false, selfMind.Session.Channel);

        }
    }

    private void OnDamage(EntityUid uid, PhantomComponent component, DamageChangedEvent args)
    {
        if (args.DamageDelta == null)
            return;

        var essenceDamage = args.DamageDelta.GetTotal().Float() * -1;

        if (args.Origin != null)
        {
            if (HasComp<HolyDamageComponent>(args.Origin))
            {
                essenceDamage = essenceDamage * component.HolyDamageMultiplier;
            }
        }

        ChangeEssenceAmount(uid, essenceDamage, component);
    }

    public bool IsHolder(EntityUid target, EntityUid? actionEntity, PhantomComponent component)
    {
        if (target == component.Holder)
        {
            _action.ClearCooldown(actionEntity);
            return true;
        }
        _action.SetCooldown(actionEntity, component.Cooldown);
        return false;
    }

    public void HauntedStopEffects(EntityUid haunted, PhantomComponent component)
    {
        if (component.ParalysisOn)
        {
            _status.TryRemoveStatusEffect(haunted, "KnockedDown");
            _status.TryRemoveStatusEffect(haunted, "Stun");
            component.ParalysisOn = false;
        }
    }

    private void OnParalysis(EntityUid uid, PhantomComponent component, ParalysisActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        var target = args.Target;

        if (IsHolder(target, component.PhantomParalysisActionEntity, component))
        {
            if (!component.ParalysisOn)
            {
                var timeHaunted = TimeSpan.FromHours(1);
                if (!_status.TryAddStatusEffect<KnockedDownComponent>(target, "KnockedDown", timeHaunted, false))
                    return;
                if (!_status.TryAddStatusEffect<StunnedComponent>(target, "Stun", timeHaunted, false))
                    return;
            }
            else
            {
                if (!_status.TryRemoveStatusEffect(target, "KnockedDown"))
                    return;
                if (!_status.TryRemoveStatusEffect(target, "Stun"))
                    return;
            }

            component.ParalysisOn = !component.ParalysisOn;
        }
        else
        {
            if (component.ParalysisOn)
            {
                _action.ClearCooldown(component.PhantomParalysisActionEntity);
                var selfMessage = Loc.GetString("phantom-paralysis-fail-active");
                _popup.PopupEntity(selfMessage, uid, uid);
                return;
            }
            else
            {
                var time = TimeSpan.FromSeconds(10);
                if (!_status.TryAddStatusEffect<KnockedDownComponent>(target, "KnockedDown", time, false))
                    return;
                if (!_status.TryAddStatusEffect<StunnedComponent>(target, "Stun", time, false))
                    return;
            }
        }
    }

    private void OnCorporeal(EntityUid uid, PhantomComponent component, MaterializeActionEvent args)
    {
        if (args.Handled)
            return;

        if (component.HasHaunted)
            return;

        args.Handled = true;

        if (TryComp<FixturesComponent>(uid, out var fixtures) && fixtures.FixtureCount >= 1)
        {
            var fixture = fixtures.Fixtures.First();

            _physicsSystem.SetCollisionMask(uid, fixture.Key, fixture.Value, (int) (CollisionGroup.SmallMobMask | CollisionGroup.GhostImpassable), fixtures);
            _physicsSystem.SetCollisionLayer(uid, fixture.Key, fixture.Value, (int) CollisionGroup.SmallMobLayer, fixtures);
        }
        var visibility = EnsureComp<VisibilityComponent>(uid);
        RemComp<AlternativeSpeechComponent>(uid);

        _visibility.SetLayer(uid, visibility, (int) VisibilityFlags.Normal, false);
        _visibility.RefreshVisibility(uid);

        component.IsCorporeal = true;
    }
}
