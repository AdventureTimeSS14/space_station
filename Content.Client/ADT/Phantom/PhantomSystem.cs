using Content.Shared.Phantom;
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
using Content.Client.UserInterface.Systems.Radial;
using Content.Client.UserInterface.Systems.Radial.Controls;
using Content.Shared.Changeling;
using Content.Shared.Humanoid.Prototypes;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Numerics;
using Content.Shared.Preferences;

namespace Content.Client.Phantom;

public sealed class PhantomSystem : EntitySystem
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

        SubscribeLocalEvent<PhantomComponent, AppearanceChangeEvent>(OnAppearanceChange);

        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeNetworkEvent<RequestPhantomStyleMenuEvent>(HandleMenuEvent);
        SubscribeNetworkEvent<RequestPhantomFreedomMenuEvent>(HandleFreedomMenuEvent);
        SubscribeNetworkEvent<RequestPhantomVesselMenuEvent>(HandleVesselMenuEvent);

        SubscribeLocalEvent<VesselComponent, CanDisplayStatusIconsEvent>(OnCanDisplayStatusIcons);
        SubscribeLocalEvent<PhantomHolderComponent, CanDisplayStatusIconsEvent>(OnCanDisplayStatusIcons);
        SubscribeLocalEvent<PhantomImmuneComponent, CanDisplayStatusIconsEvent>(OnCanDisplayStatusIcons);
    }

    #region Radial Menu

    private RadialContainer? _openedMenu;

    private const string DefaultIcon = "/Textures/Interface/AdminActions/play.png";

    private const string EmptyIcon = "/Textures/Interface/AdminActions/empty.png";

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        _openedMenu?.Dispose();
    }

    private void OnPlayerDetached(PlayerDetachedEvent args)
    {
        _openedMenu?.Dispose();
    }

    /// <summary>
    /// Draws RadialUI.
    /// <seealso cref="Content.Client.UserInterface.Systems.Radial.RadialUiController"/>
    /// </summary>
    private void HandleMenuEvent(RequestPhantomStyleMenuEvent args)
    {
        if (_openedMenu != null)
            return;

        _openedMenu = _userInterfaceManager.GetUIController<RadialUiController>()
            .CreateRadialContainer();

        foreach (var protoId in args.Prototypes)
        {
            if (_proto.TryIndex<PhantomStylePrototype>(protoId, out var prototype))
            {
                var actionName = prototype.Name;
                var texturePath = _spriteSystem.Frame0(new SpriteSpecifier.Texture(new ResPath(DefaultIcon)));
                if (actionName == null)
                    actionName = "Unnamed";
                if (prototype.Icon != null)
                    texturePath = _spriteSystem.Frame0(prototype.Icon);

                var emoteButton = _openedMenu.AddButton(actionName, texturePath);
                emoteButton.Opacity = 210;
                emoteButton.Tooltip = null;
                emoteButton.Controller.OnPressed += (_) =>
                {
                    var ev = new SelectPhantomStyleEvent(args.Target, protoId);
                    RaiseNetworkEvent(ev);
                    _openedMenu.Dispose();
                };
            }
        }

        _openedMenu.OnClose += (_) =>
        {
            _openedMenu = null;
        };
        if (_playerMan.LocalEntity != null)
            _openedMenu.OpenAttached(_playerMan.LocalEntity.Value);

    }

    private void HandleFreedomMenuEvent(RequestPhantomFreedomMenuEvent args)
    {
        if (_openedMenu != null)
            return;

        _openedMenu = _userInterfaceManager.GetUIController<RadialUiController>()
            .CreateRadialContainer();

        foreach (var protoId in args.Prototypes)
        {
            if (_proto.TryIndex<EntityPrototype>(protoId, out var prototype))
            {
                if (!prototype.TryGetComponent<InstantActionComponent>(out var actionComp))
                    continue;

                var actionName = prototype.Name;
                var texturePath = actionComp.Icon;
                if (actionName == null)
                    actionName = "Unnamed";
                if (texturePath == null)
                    continue;

                var emoteButton = _openedMenu.AddButton(actionName, _spriteSystem.Frame0(texturePath));
                emoteButton.Opacity = 210;
                emoteButton.Tooltip = null;
                emoteButton.Controller.OnPressed += (_) =>
                {
                    var ev = new SelectPhantomFreedomEvent(args.Target, protoId);
                    RaiseNetworkEvent(ev);
                    _openedMenu.Dispose();
                };
            }
        }

        _openedMenu.OnClose += (_) =>
        {
            _openedMenu = null;
        };
        if (_playerMan.LocalEntity != null)
            _openedMenu.OpenAttached(_playerMan.LocalEntity.Value);

    }

    private void HandleVesselMenuEvent(RequestPhantomVesselMenuEvent args)
    {
        if (_openedMenu != null)
            return;

        _openedMenu = _userInterfaceManager.GetUIController<RadialUiController>()
            .CreateRadialContainer();

        foreach (var (vessel, humanoid, name) in args.Vessels)
        {
            var dummy = _entManager.SpawnEntity(_proto.Index<SpeciesPrototype>(humanoid.Species).DollPrototype, MapCoordinates.Nullspace);
            //var humanoidEntityUid = GetEntity(humanoid); // Entities on the client outside of the FOV are nonexistant. You can see that if you zoom out. //So it'll give you UID 0 which is EntityUid.Invalid.
            _appearanceSystem.LoadProfile(dummy, humanoid);
            var face = new SpriteView();
            face.SetEntity(dummy);

            var actionName = name;
            var texturePath = _spriteSystem.Frame0(new SpriteSpecifier.Texture(new ResPath(EmptyIcon)));

            var button = _openedMenu.AddButton(actionName, texturePath, face);
            button.Opacity = 210;
            button.Tooltip = null;
            button.Controller.OnPressed += (_) =>
            {
                var ev = new SelectPhantomVesselEvent(args.Uid, vessel);
                RaiseNetworkEvent(ev);
                _openedMenu.Dispose();
            };
        }

        _openedMenu.OnClose += (_) =>
        {
            _openedMenu = null;
        };
        if (_playerMan.LocalEntity != null)
            _openedMenu.OpenAttached(_playerMan.LocalEntity.Value);

    }
    #endregion


    private void OnCanDisplayStatusIcons<T>(EntityUid uid, T component, ref CanDisplayStatusIconsEvent args) where T : IAntagStatusIconComponent
    {
        if (HasComp<PhantomComponent>(args.User) || HasComp<PhantomPuppetComponent>(args.User) || HasComp<ShowVesselIconsComponent>(args.User))
            return;

        if (component.IconVisibleToGhost && HasComp<GhostComponent>(args.User))
            return;

        args.Cancelled = true;
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
        else if (_appearance.TryGetData<bool>(uid, PhantomVisuals.Stunned, out var stunned, args.Component) && stunned)
        {
            args.Sprite.LayerSetState(0, component.StunnedState);
        }
        else if (_appearance.TryGetData<bool>(uid, PhantomVisuals.Corporeal, out var corporeal, args.Component))
        {
            if (corporeal)
                args.Sprite.LayerSetState(0, component.CorporealState);
            else
                args.Sprite.LayerSetState(0, component.State);
        }
    }
}
