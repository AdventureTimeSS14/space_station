using Content.Shared.Popups;
using Content.Shared.Pulling;
using Content.Shared.Pulling.Components;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Shared.Phantom.Components;

namespace Content.Shared.Phantom;

/// <summary>
/// This handles teleporting entities through portals, and creating new linked portals.
/// </summary>
public sealed partial class SharedPhantomPortalSystem : EntitySystem
{
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPullingSystem _pulling = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<PhantomPortalComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(EntityUid uid, PhantomPortalComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess)
            return;

        if (Deleted(component.LinkedPortal))
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Priority = 11,
            Act = () =>
            {
                if (component.LinkedPortal == null || disabled)
                    return;

                var ent = component.LinkedPortal.Value;
                TeleportEntity(uid, args.User, Transform(ent).Coordinates, ent, false);
            },
            Text = Loc.GetString("portal-component-ghost-traverse"),
            Message = disabled
                ? Loc.GetString("portal-component-no-linked-entities-phant")
                : Loc.GetString("portal-component-can-ghost-traverse"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/open.svg.192dpi.png"))
        });
    }

    private void TeleportEntity(EntityUid portal, EntityUid subject, EntityCoordinates target, EntityUid? targetEntity = null, bool playSound = true,
        PhantomPortalComponent? portalComponent = null)
    {
        if (!Resolve(portal, ref portalComponent))
            return;

        var ourCoords = Transform(portal).Coordinates;
        var onSameMap = ourCoords.GetMapId(EntityManager) == target.GetMapId(EntityManager);
        var distanceInvalid = portalComponent.MaxTeleportRadius != null
                              && ourCoords.TryDistance(EntityManager, target, out var distance)
                              && distance > portalComponent.MaxTeleportRadius;

        if (!onSameMap || distanceInvalid)
        {
            if (!_netMan.IsServer)
                return;

            // Early out if this is an invalid configuration
            _popup.PopupCoordinates(Loc.GetString("portal-component-invalid-configuration-fizzle"),
                ourCoords, Filter.Pvs(ourCoords, entityMan: EntityManager), true);

            _popup.PopupCoordinates(Loc.GetString("portal-component-invalid-configuration-fizzle"),
                target, Filter.Pvs(target, entityMan: EntityManager), true);

            QueueDel(portal);

            if (targetEntity != null)
                QueueDel(targetEntity.Value);

            return;
        }

        if (TryComp<SharedPullableComponent>(subject, out var pullable) && pullable.BeingPulled)
        {
            _pulling.TryStopPull(pullable);
        }

        if (TryComp<SharedPullerComponent>(subject, out var pulling)
            && pulling.Pulling != null && TryComp<SharedPullableComponent>(pulling.Pulling.Value, out var subjectPulling))
        {
            _transform.SetCoordinates(pulling.Pulling.Value, target);
        }

        _transform.SetCoordinates(subject, target);

        if (!playSound)
            return;

    }
}
