using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Overlays;
using Content.Shared.StatusIcon.Components;

namespace Content.Client.Overlays;

public sealed class ShowHungerIconsSystem : EquipmentHudSystem<ShowHungerIconsComponent>
{
    [Dependency] private readonly HungerSystem _hunger = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HungerComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(EntityUid uid, HungerComponent component, ref GetStatusIconsEvent ev)
    {
<<<<<<< HEAD
        if (!IsActive || args.InContainer || args.HasStealthComponent)
=======
        if (!IsActive)
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
            return;

        if (_hunger.TryGetStatusIconPrototype(component, out var iconPrototype))
            ev.StatusIcons.Add(iconPrototype);
    }
}
