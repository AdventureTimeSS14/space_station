using Content.Shared.Phantom;
using Content.Shared.Phantom.Components;
using Robust.Client.GameObjects;

namespace Content.Client.Revenant;

public sealed class PhantomSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhantomComponent, AppearanceChangeEvent>(OnAppearanceChange);
    }

    private void OnAppearanceChange(EntityUid uid, PhantomComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        if (_appearance.TryGetData<bool>(uid, PhantomVisuals.Haunting, out var haunt, args.Component))
        {
            if (haunt)
                args.Sprite.LayerSetState(0, component.HauntingState);
            else
                args.Sprite.LayerSetState(0, component.State);
        }
    }
}
