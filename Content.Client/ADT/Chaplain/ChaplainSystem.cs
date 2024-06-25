using Content.Shared.Bible.Components;
using Content.Shared.Phantom.Components;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Client.UserInterface.Systems.Radial;
using Content.Client.UserInterface.Systems.Radial.Controls;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Client.UserInterface;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Ghost;
using Content.Shared.Antag;
using Content.Shared.Actions;
using Robust.Client.Graphics;
using Robust.Client.Utility;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Prototypes;
using Content.Client.Humanoid;
using System.Numerics;
using Content.Shared.Preferences;

namespace Content.Client.Chaplain;

public sealed class ChaplainSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;
    [Dependency] private readonly HumanoidAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChaplainComponent, CanDisplayStatusIconsEvent>(OnCanDisplayStatusIcons);
    }

    private void OnCanDisplayStatusIcons(EntityUid uid, ChaplainComponent component, ref CanDisplayStatusIconsEvent args)
    {
        if (HasComp<PhantomComponent>(args.User) || HasComp<ChaplainComponent>(args.User) || HasComp<ShowChaplainIconsComponent>(args.User))
            return;

        if (component.IconVisibleToGhost && HasComp<GhostComponent>(args.User))
            return;

        args.Cancelled = true;
    }
}
