using Content.Shared.CCVar;
using Content.Shared.Ghost;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Content.Shared.Stealth.Components;
<<<<<<< HEAD
=======
using Content.Shared.Whitelist;
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;

namespace Content.Client.StatusIcon;

/// <summary>
/// This handles rendering gathering and rendering icons on entities.
/// </summary>
public sealed class StatusIconSystem : SharedStatusIconSystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly IOverlayManager _overlay = default!;
<<<<<<< HEAD
    [Dependency] private readonly IEntityManager _entManager = default!;
=======
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly EntityWhitelistSystem _entityWhitelist = default!;
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2

    private bool _globalEnabled;
    private bool _localEnabled;

    /// <inheritdoc/>
    public override void Initialize()
    {
        Subs.CVar(_configuration, CCVars.LocalStatusIconsEnabled, OnLocalStatusIconChanged, true);
        Subs.CVar(_configuration, CCVars.GlobalStatusIconsEnabled, OnGlobalStatusIconChanged, true);
    }

    private void OnLocalStatusIconChanged(bool obj)
    {
        _localEnabled = obj;
        UpdateOverlayVisible();
    }

    private void OnGlobalStatusIconChanged(bool obj)
    {
        _globalEnabled = obj;
        UpdateOverlayVisible();
    }

    private void UpdateOverlayVisible()
    {
        if (_overlay.RemoveOverlay<StatusIconOverlay>())
            return;

        if (_globalEnabled && _localEnabled)
            _overlay.AddOverlay(new StatusIconOverlay());
    }

    public List<StatusIconData> GetStatusIcons(EntityUid uid, MetaDataComponent? meta = null)
    {
        var list = new List<StatusIconData>();
        if (!Resolve(uid, ref meta))
            return list;

        if (meta.EntityLifeStage >= EntityLifeStage.Terminating)
            return list;

<<<<<<< HEAD
        var inContainer = (meta.Flags & MetaDataFlags.InContainer) != 0;
        var hasStealthComponent = _entManager.HasComponent<StealthComponent>(uid);
        var ev = new GetStatusIconsEvent(list, inContainer, hasStealthComponent);
=======
        var ev = new GetStatusIconsEvent(list);
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
        RaiseLocalEvent(uid, ref ev);
        return ev.StatusIcons;
    }

    /// <summary>
    /// For overlay to check if an entity can be seen.
    /// </summary>
    public bool IsVisible(Entity<MetaDataComponent> ent, StatusIconData data)
    {
        var viewer = _playerManager.LocalSession?.AttachedEntity;

        // Always show our icons to our entity
        if (viewer == ent.Owner)
            return true;

        if (data.VisibleToGhosts && HasComp<GhostComponent>(viewer))
            return true;

        if (data.HideInContainer && (ent.Comp.Flags & MetaDataFlags.InContainer) != 0)
            return false;

        if (data.HideOnStealth && TryComp<StealthComponent>(ent, out var stealth) && stealth.Enabled)
            return false;

        if (data.ShowTo != null && !_entityWhitelist.IsValid(data.ShowTo, viewer))
            return false;

        return true;
    }
}
