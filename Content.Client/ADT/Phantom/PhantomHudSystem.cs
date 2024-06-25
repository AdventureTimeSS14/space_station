using Content.Shared.Overlays;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Phantom.Components;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;
using Content.Client.Overlays;

namespace Content.Client.Phantom;
public sealed class ShowHauntedIconsSystem : EquipmentHudSystem<ShowHauntedIconsComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhantomHolderIconComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, PhantomHolderIconComponent haunted, ref GetStatusIconsEvent args)
    {
        if (!IsActive || args.InContainer)
        {
            return;
        }

        var syndicateIcons = SyndicateIcon(uid, haunted);

        args.StatusIcons.AddRange(syndicateIcons);
    }

    private IReadOnlyList<StatusIconPrototype> SyndicateIcon(EntityUid uid, PhantomHolderIconComponent haunted)
    {
        var result = new List<StatusIconPrototype>();

        if (_prototype.TryIndex<StatusIconPrototype>(haunted.StatusIcon, out var icon))
        {
            result.Add(icon);
        }

        return result;
    }
}

