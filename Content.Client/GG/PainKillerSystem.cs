using Content.Shared.Drugs;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Content.Shared.Alert;
using Content.Shared.GG.Drugs;
using Content.Client.UserInterface.Systems.DamageOverlays;

namespace Content.Client.GG.Drugs;

public sealed class PainKillerSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;
    public static string PainKillerKey = "PainKiller";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PainKillerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<PainKillerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<PainKillerComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PainKillerComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

    }

    private void OnInit(EntityUid uid, PainKillerComponent component, ComponentInit args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
            _alertsSystem.ShowAlert(uid, AlertType.PainKiller);
    }

    private void OnShutdown(EntityUid uid, PainKillerComponent component, ComponentShutdown args)
    {
        if (_player.LocalPlayer?.ControlledEntity == uid)
            _alertsSystem.ClearAlert(uid, AlertType.PainKiller);
    }

    private void OnPlayerAttached(EntityUid uid, PainKillerComponent component, LocalPlayerAttachedEvent args)
    {
        _alertsSystem.ShowAlert(uid, AlertType.PainKiller);
    }

    private void OnPlayerDetached(EntityUid uid, PainKillerComponent component, LocalPlayerDetachedEvent args)
    {
        _alertsSystem.ClearAlert(uid, AlertType.PainKiller);
    }


}
