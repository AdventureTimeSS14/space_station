using Content.Server.Actions;
using Content.Server.Chat.Systems;
using Content.Shared.ADT.EmotePanel;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Chat.Prototypes;
using Content.Server.Emoting.Components;
using Content.Server.Speech.Components;

namespace Content.Server.ADT.EmotePanel;

public sealed class EmotePanelSystem: EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmotePanelComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EmotePanelComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<EmotePanelComponent, OpenEmotesActionEvent>(OnEmotingAction);

        SubscribeNetworkEvent<SelectEmoteEvent>(OnSelectEmote);
    }



    private void OnMapInit(EntityUid uid, EmotePanelComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.OpenEmotesActionEntity, component.OpenEmotesAction);
    }
    private void OnShutdown(EntityUid uid, EmotePanelComponent component, ComponentShutdown args)
    {
        throw new NotImplementedException();
    }
    private void OnEmotingAction(EntityUid uid, EmotePanelComponent component, OpenEmotesActionEvent args)
    {
        if (args.Handled)
            return;

        if (EntityManager.TryGetComponent<ActorComponent?>(uid, out var actorComponent))
        {
            var ev = new RequestEmoteMenuEvent(uid.Id);

            foreach (var prototype in _proto.EnumeratePrototypes<EmotePrototype>())
            {
                // NOTE: Maybe need make some value in configuration.
                // If TRUE, we can put in menu next emotes, like: Meows, Honk, Heezes and something.
                // Or we can put only those emotes, what we can trigger with chat
                if (prototype.ChatTriggers.Count <= 0)
                    continue;
                if (prototype.Icon == null)
                    continue;

                switch (prototype.Category)
                {
                    case EmoteCategory.General:
                        ev.Prototypes.Add(prototype.ID);
                        break;
                    case EmoteCategory.Hands:
                        if (EntityManager.TryGetComponent<BodyEmotesComponent>(uid, out var _))
                            ev.Prototypes.Add(prototype.ID);
                        break;
                    case EmoteCategory.Vocal:
                        if (EntityManager.TryGetComponent<VocalComponent>(uid, out var _))
                            ev.Prototypes.Add(prototype.ID);
                        break;
                }
            }
            ev.Prototypes.Sort();
            RaiseNetworkEvent(ev, actorComponent.PlayerSession);
        }

        args.Handled = true;
    }
    private void OnSelectEmote(SelectEmoteEvent msg)
    {
        _chat.TryEmoteWithChat(new EntityUid(msg.Target), msg.PrototypeId);
    }

}
