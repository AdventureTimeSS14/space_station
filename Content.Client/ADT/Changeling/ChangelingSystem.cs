using Content.Client.UserInterface.Systems.Radial;
using Content.Client.UserInterface.Systems.Radial.Controls;
using Content.Shared.Changeling;
using Content.Shared.Chat.Prototypes;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Robust.Client.UserInterface.Controls;
using System.Numerics;

namespace Content.Client.Changeling;

public sealed class TransformPanelSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IPlayerManager _playerMan = default!;
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    /// <summary>
    /// We should enable radial for single target
    /// </summary>
    private RadialContainer? _openedMenu;

    private const string DefaultIcon = "/Textures/Interface/AdminActions/play.png";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeNetworkEvent<RequestTransformMenuEvent>(HandleTransformMenuEvent);
    }

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
    private void HandleTransformMenuEvent(RequestTransformMenuEvent args)
    {
        if (_openedMenu != null)
            return;

        _openedMenu = _userInterfaceManager.GetUIController<RadialUiController>()
            .CreateRadialContainer();

        foreach (var dna in args.StoredDNAs)
        {
            if (TryComp<HumanoidAppearanceComponent>(dna.EntityUid, out var humanoid) && TryComp<MetaDataComponent>(dna.EntityUid, out var meta))
            {
                var actionName = meta.EntityName;
                //var texturePath = humanoid.;


                //if (prototype.Icon != null)
                //    texturePath = _spriteSystem.Frame0(prototype.Icon);
                if (EntityManager.HasComponent<SpriteComponent>(dna.EntityUid))
                {
                    var spriteView = new SpriteView
                    {
                        OverrideDirection = Direction.South,
                        SetSize = new Vector2(32, 32),
                        Margin = new Thickness(2, 0, 2, 0),
                    };
                    //var texturePath = _spriteSystem.Frame0(spriteView);

                    spriteView.SetEntity(dna.EntityUid);
                    var emoteButton = _openedMenu.AddButton(actionName);
                    emoteButton.Opacity = 210;
                    emoteButton.Tooltip = null;
                    emoteButton.AddChild(spriteView);
                    emoteButton.Controller.OnPressed += (_) =>
                    {
                        var ev = new SelectTransformEvent(args.Target, dna);
                        RaiseNetworkEvent(ev);
                        _openedMenu.Dispose();
                    };
                }

            }
        }

        _openedMenu.OnClose += (_) =>
        {
            _openedMenu = null;
        };
        if (_playerMan.LocalEntity != null)
            _openedMenu.OpenAttached((EntityUid) _playerMan.LocalEntity);

    }
}
