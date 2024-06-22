using System.Linq;
using Content.Server.Popups;
using Content.Server.PowerCell;
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Language;
using Content.Shared.Language.Components;
using Content.Shared.Language.Systems;
using Content.Shared.PowerCell;
using Content.Shared.GhostInteractions;

namespace Content.Server.GhostInteractions;

// this does not support holding multiple translators at once yet.
// that should not be an issue for now, but it better get fixed later.
public sealed class GhostRadioSystem : SharedGhostRadioSystem
{
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PowerCellSystem _powerCell = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GhostRadioComponent, ActivateInWorldEvent>(OnTranslatorToggle);
        SubscribeLocalEvent<GhostRadioComponent, PowerCellSlotEmptyEvent>(OnPowerCellSlotEmpty);
    }

    private void OnTranslatorToggle(EntityUid translator, GhostRadioComponent component, ActivateInWorldEvent args)
    {
        if (!component.ToggleOnInteract)
            return;

        var hasPower = _powerCell.HasDrawCharge(translator);

            var isEnabled = !component.Enabled;

            isEnabled &= hasPower;
            component.Enabled = isEnabled;
            _powerCell.SetPowerCellDrawEnabled(translator, isEnabled);

        OnAppearanceChange(translator, component);

        // HasPower shows a popup when there's no power, so we do not proceed in that case
        if (hasPower)
        {
            var message =
                Loc.GetString(component.Enabled ? "ghost-radio-component-turnon" : "ghost-radio-component-shutoff");
            _popup.PopupEntity(message, component.Owner, args.User);
        }
    }

    private void OnPowerCellSlotEmpty(EntityUid translator, GhostRadioComponent component, PowerCellSlotEmptyEvent args)
    {
        component.Enabled = false;
        _powerCell.SetPowerCellDrawEnabled(translator, false);
        OnAppearanceChange(translator, component);
    }
}
