using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Content.Shared.ADT.Components;
using Content.Client.ADT.Overlays.Shaders;

namespace Content.Client.ADT.Systems;

/// <summary>
///     System to handle the SeeingStatic overlay.
/// </summary>
public sealed class AnomStaticSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private AnomStaticOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnomStaticComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<AnomStaticComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<AnomStaticComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<AnomStaticComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(EntityUid uid, AnomStaticComponent component, LocalPlayerAttachedEvent args)
    {
        _overlayMan.AddOverlay(_overlay);
    }

    private void OnPlayerDetached(EntityUid uid, AnomStaticComponent component, LocalPlayerDetachedEvent args)
    {
        _overlay.MixAmount = 0;
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnInit(EntityUid uid, AnomStaticComponent component, ComponentInit args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
            _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(EntityUid uid, AnomStaticComponent component, ComponentShutdown args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
        {
            _overlay.MixAmount = 0;
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
