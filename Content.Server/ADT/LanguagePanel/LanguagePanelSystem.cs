/// Made for Adventure Time Project by ModerN. https://github.com/modern-nm mailto:modern-nm@yandex.by
/// see also https://github.com/DocNITE/liebendorf-station/tree/feature/emote-radial-panel
using Content.Server.Actions;
using Content.Server.Chat.Systems;
using Content.Shared.ADT.EmotePanel;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Chat.Prototypes;
using Content.Server.Emoting.Components;
using Content.Server.Speech.Components;
using Content.Shared.ADT.LanguagePanel;
using Content.Shared.Language;
using Content.Server.Language;

namespace Content.Server.ADT.LanguagePanel;

/// <summary>
/// LanguagePanelSystem process actions on "ActionOpenEmotes" and RadialUi.
/// <see cref="Content.Shared.ADT.LanguagePanel.LanguagePanelComponent"/>
/// </summary>
public sealed class LanguagePanelSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LanguagePanelComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<LanguagePanelComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<LanguagePanelComponent, OpenLanguagesActionEvent>(OnLangAction);

        SubscribeNetworkEvent<SelectLanguageEvent>(OnSelectLanguage);
    }

    private void OnSelectLanguage(SelectLanguageEvent ev)
    {
        var languages = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<LanguageSystem>();
        languages.SetLanguage(new EntityUid(ev.Target), ev.PrototypeId);
    }

    private void OnLangAction(EntityUid uid, LanguagePanelComponent component, OpenLanguagesActionEvent args)
    {
        if (args.Handled)
            return;
        if (!TryComp<ActorComponent>(uid, out var actor))
            return;

        var player = actor.PlayerSession;

        if (EntityManager.TryGetComponent<LanguageSpeakerComponent>(uid, out var languageSpeakerComponent))
        {
            var ev = new RequestLanguageMenuEvent(uid.Id);//////////// to net-entities
            foreach (var lang in languageSpeakerComponent.SpokenLanguages)
            {
                ev.Languages.Add(lang);
            }
            RaiseNetworkEvent(ev, player);
        }

        args.Handled = true;
    }

    private void OnShutdown(EntityUid uid, LanguagePanelComponent component, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, component.OpenLanguagesActionEntity);
    }

    private void OnMapInit(EntityUid uid, LanguagePanelComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.OpenLanguagesActionEntity, component.OpenLanguagesAction);
    }
}
