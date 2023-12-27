using Content.Shared.GameTicking;
using Content.Shared.Sirena.EntityHealthBar;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client.Sirena.EntityHealthBar
{
    public sealed class ShowHealthBarsSystem : EntitySystem
    {
        [Dependency] private readonly IPlayerManager _player = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IOverlayManager _overlayMan = default!;

        private EntityHealthBarOverlay _overlay = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ShowHealthBarsComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<ShowHealthBarsComponent, ComponentRemove>(OnRemove);
            SubscribeLocalEvent<ShowHealthBarsComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
            SubscribeLocalEvent<ShowHealthBarsComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);
            SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);

            _overlay = new(EntityManager, _protoMan);
        }

        private void OnInit(EntityUid uid, ShowHealthBarsComponent component, ComponentInit args)
        {
            if (_player.LocalPlayer?.ControlledEntity == uid)
            {
                _overlayMan.AddOverlay(_overlay);
                _overlay.DamageContainer = component.DamageContainer;
            }


        }
        private void OnRemove(EntityUid uid, ShowHealthBarsComponent component, ComponentRemove args)
        {
            if (_player.LocalPlayer?.ControlledEntity == uid)
            {
                _overlayMan.RemoveOverlay(_overlay);
            }
        }

        private void OnPlayerAttached(EntityUid uid, ShowHealthBarsComponent component, LocalPlayerAttachedEvent args)
        {
            _overlayMan.AddOverlay(_overlay);
            _overlay.DamageContainer = component.DamageContainer;
        }

        private void OnPlayerDetached(EntityUid uid, ShowHealthBarsComponent component, LocalPlayerDetachedEvent args)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }

        private void OnRoundRestart(RoundRestartCleanupEvent args)
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
