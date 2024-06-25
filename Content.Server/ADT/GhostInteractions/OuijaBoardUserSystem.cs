using System.Linq;
using Content.Server.Popups;
using Content.Server.PowerCell;
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Language;
using Content.Shared.Language.Components;
using Content.Shared.Language.Systems;
using Content.Server.Tabletop;
using Content.Shared.GhostInteractions;
using Content.Server.Light.Components;
using Content.Server.Ghost;
using Content.Server.Tabletop.Components;
using Robust.Shared.Player;

namespace Content.Server.GhostInteractions;

// this does not support holding multiple translators at once yet.
// that should not be an issue for now, but it better get fixed later.
public sealed class OuijaBoardUserSystem : EntitySystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TabletopSystem _tabletop = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<OuijaBoardUserComponent, InteractNoHandEvent>(OnInteract);
    }
    private void OnInteract(EntityUid uid, OuijaBoardUserComponent component, InteractNoHandEvent args)
    {
        if (args.Target == args.User || args.Target == null)
            return;
        var target = args.Target.Value;

        if (HasComp<PoweredLightComponent>(target))
        {
            args.Handled = _ghost.DoGhostBooEvent(target);
            return;
        }
        if (HasComp<OuijaBoardComponent>(target) && HasComp<TabletopGameComponent>(target))
        {
            if (!TryComp<ActorComponent>(uid, out var actor))
                return;

            _tabletop.OpenSessionFor(actor.PlayerSession, target);
        }
    }

}
