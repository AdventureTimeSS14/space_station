using Content.Shared.Changeling.Components;
using Content.Shared.Changeling;
using Content.Shared.Inventory;
using Content.Server.Hands.Systems;
using Robust.Shared.Prototypes;
using Content.Server.Body.Systems;
using Content.Shared.Popups;
using Content.Shared.IdentityManagement;
using Robust.Shared.Audio.Systems;
using Content.Server.Emp;
using Content.Shared.DoAfter;
using Content.Shared.Humanoid;
using Content.Server.Fluids.EntitySystems;

namespace Content.Server.Changeling.EntitySystems;

public sealed partial class LingSlugSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

    private void InitializeLingAbilities()
    {
        SubscribeLocalEvent<LingSlugComponent, LingEggActionEvent>(OnLayEggs);
        SubscribeLocalEvent<LingSlugComponent, LingEggDoAfterEvent>(OnLayEggsDoAfter);
    }

    private void OnLayEggs(EntityUid uid, LingSlugComponent component, LingEggActionEvent args)     /// TODO: Заменить на кладку яиц при ударе.
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            var selfMessage = Loc.GetString("changeling-dna-fail-nohuman", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        if (!_mobState.IsIncapacitated(target)) // if target isn't crit or dead dont let absorb
        {
            var selfMessage = Loc.GetString("changeling-dna-fail-notdead", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        if (HasComp<AbsorbedComponent>(target))
        {
            var selfMessage = Loc.GetString("changeling-dna-alreadyabsorbed", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        if (_tagSystem.HasTag(target, "ChangelingBlacklist"))
        {
            var selfMessage = Loc.GetString("changeling-dna-sting-fail-nodna", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        if (LayEggs(uid, target, component))
        {
            args.Handled = true;

            var selfMessage = Loc.GetString("changeling-eggs-self-start", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid, PopupType.MediumCaution);
        }

    }

    private void OnLayEggsDoAfter(EntityUid uid, LingSlugComponent component, LingEggDoAfterEvent args)
    {
        if (args.Handled || args.Args.Target == null)
            return;

        args.Handled = true;
        var target = args.Args.Target.Value;

        if (args.Cancelled || !_mobState.IsIncapacitated(target) || HasComp<AbsorbedComponent>(target))
        {
            var selfMessage = Loc.GetString("changeling-eggs-interrupted");
            _popup.PopupEntity(selfMessage, uid, uid);
            return;
        }

        else
        {
            var holderComp = EnsureComp<LingEggsHolderComponent>(target);

            var selfMessage = Loc.GetString("changeling-eggs-self-success", ("target", Identity.Entity(target, EntityManager)));
            _popup.PopupEntity(selfMessage, uid, uid, PopupType.MediumCaution);

            component.EggsLaid = true;
            component.EggLing = target;

            holderComp.ChangelingHatchAction = component.HatchAction;

            _action.RemoveAction(uid, component.LayEggsActionEntity);   /// Яйца откладываются только один раз

            return;
        }
    }
}
