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
using Content.Shared.Nutrition.Components;
using Content.Shared.Tag;
using Content.Shared.StatusEffect;
using Content.Shared.Movement.Systems;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Content.Server.Stunnable;
using Content.Shared.Mind;
using Content.Shared.Humanoid;
using Robust.Shared.Containers;

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

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhantomComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<PhantomComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PhantomComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(EntityUid uid, PhantomComponent component, ComponentStartup args)
    {

    }

    public bool ChangeEssenceAmount(EntityUid uid, float amount, PhantomComponent? component = null, bool regenCap = true)
    {
        if (!Resolve(uid, ref component))
            return false;

        component.Essence += amount;

        if (regenCap)
            float.Min(component.Essence, component.EssenceRegenCap);

        _alerts.ShowAlert(uid, AlertType.Chemicals, (short) Math.Clamp(Math.Round(component.Essence / 10.7f), 0, 7));

        return true;
    }

    private bool TryUseAbility(EntityUid uid, PhantomComponent component, float abilityCost, bool activated = true, float regenCost = 0f)
    {
        if (component.Essence <= Math.Abs(abilityCost) && activated)
        {
            _popup.PopupEntity(Loc.GetString("changeling-not-enough-chemicals"), uid, uid);
            return false;
        }

        if (activated)
        {
            ChangeEssenceAmount(uid, abilityCost, component, false);
            component.EssencePerSecond -= regenCost;
        }
        else
        {
            component.EssencePerSecond += regenCost;
        }

        return true;
    }

    private void OnMapInit(EntityUid uid, PhantomComponent component, MapInitEvent args)
    {
    }

    private void OnShutdown(EntityUid uid, PhantomComponent component, ComponentShutdown args)
    {
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<PhantomComponent>();
        while (query.MoveNext(out var uid, out var ling))
        {
            ling.Accumulator += frameTime;

            if (ling.Accumulator <= 1)
                continue;
            ling.Accumulator -= 1;

            if (ling.Essence < ling.EssenceRegenCap)
            {
                ChangeEssenceAmount(uid, ling.EssencePerSecond, ling, regenCap: true);
            }

        }
    }

    private bool TryHaunt(EntityUid uid, EntityUid target, PhantomComponent component)
    {
        if (HasComp<PhantomHolderComponent>(target))
        {
            var selfMessage = Loc.GetString("phantom-haunt-fail-self", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);
            return false;
        }

        return true;
    }

    private void OnMakeHolder(EntityUid uid, PhantomComponent component, MakeHolderActionEvent args)     // Жало кражи днк
    {
        if (args.Handled)
            return;

        var target = args.Target;

        var holder = EnsureComp<PhantomHolderComponent>(target);

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            var selfMessageFailNoHuman = Loc.GetString("changeling-dna-fail-nohuman", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessageFailNoHuman, uid, uid);
            return;
        }

        if (!TryUseAbility(uid, component, 15f))
            return;

        if (TryHaunt(uid, target, component))
        {
            args.Handled = true;

            component.Holder = target;

            ContainerSystem.Insert(uid, holder.Container);

            var selfMessage = Loc.GetString("phantom-haunt-self", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);

            var targetMessage = Loc.GetString("phantom-haunt-target");
            _popup.PopupEntity(targetMessage, target, target);
        }
    }
}
