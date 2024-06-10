using Content.Shared.Drugs;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Content.Shared.Alert;
using Content.Client.UserInterface.Systems.DamageOverlays;
using Content.Shared.ADT;
using Content.Shared.Changeling.Components;

namespace Content.Client.Changeling;

public sealed class LingEggsSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    public static string EggKey = "LingEggs";

    private void InitializeAlert()
    {

        SubscribeLocalEvent<LingEggsHolderComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<LingEggsHolderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<LingEggsHolderComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<LingEggsHolderComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

    }

    private void OnInit(EntityUid uid, LingEggsHolderComponent component, ComponentInit args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
            _alertsSystem.ShowAlert(uid, component.Alert);
    }

    private void OnShutdown(EntityUid uid, LingEggsHolderComponent component, ComponentShutdown args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
            _alertsSystem.ClearAlert(uid, component.Alert);
    }

    private void OnPlayerAttached(EntityUid uid, LingEggsHolderComponent component, LocalPlayerAttachedEvent args)
    {
        _alertsSystem.ShowAlert(uid, component.Alert);
    }

    private void OnPlayerDetached(EntityUid uid, LingEggsHolderComponent component, LocalPlayerDetachedEvent args)
    {
        _alertsSystem.ClearAlert(uid, component.Alert);
    }
}
