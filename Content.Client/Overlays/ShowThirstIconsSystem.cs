using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Overlays;
using Content.Shared.StatusIcon.Components;

namespace Content.Client.Overlays;

public sealed class ShowThirstIconsSystem : EquipmentHudSystem<ShowThirstIconsComponent>
{
    [Dependency] private readonly ThirstSystem _thirst = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThirstComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, ThirstComponent component, ref GetStatusIconsEvent ev)
    {
<<<<<<< HEAD
        if (!IsActive || args.InContainer || args.HasStealthComponent)
=======
        if (!IsActive)
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
            return;

        if (_thirst.TryGetStatusIconPrototype(component, out var iconPrototype))
            ev.StatusIcons.Add(iconPrototype!);
    }
}
