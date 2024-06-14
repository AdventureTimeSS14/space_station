using Content.Server.Administration.Commands;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Mobs;
using Content.Server.Chat;
using Content.Server.Chat.Systems;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Stunnable;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Content.Server.Emoting.Systems;
using Content.Server.Speech.EntitySystems;
using Content.Shared.ADT.AutoPostingChat;
using Content.Shared.Interaction.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using System.Timers;
using System.ComponentModel;
using System.Linq;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
public sealed class AutoPostingChatSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    private System.Timers.Timer _speakTimer = new();
    private System.Timers.Timer _emoteTimer = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AutoPostingChatComponent, ComponentStartup>(OnComponentStartup);
        SubscribeLocalEvent<AutoPostingChatComponent, MobStateChangedEvent>(OnMobState);
        SubscribeLocalEvent<AutoPostingChatComponent, ComponentShutdown>(ComponentRemove);
    }

    private void ComponentRemove(EntityUid uid, AutoPostingChatComponent component, ComponentShutdown args)
    {
        _speakTimer.Stop();
        _emoteTimer.Stop();
        _speakTimer.Dispose(); // освобождаем ресурсы
        _emoteTimer.Dispose();
    }

    /// <summary>
    /// On death removes active comps.
    /// </summary>
    private void OnMobState(EntityUid uid, AutoPostingChatComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead || component == null)
        {
            RemComp<AutoPostingChatComponent>(uid);
        }
    }

    private void OnComponentStartup(EntityUid uid, AutoPostingChatComponent component, ComponentStartup args)
    {
       // Проверяем наличие компонента AutoPostingChatComponent на сущности
       if (component == null)
        {
            Log.Debug("AutoPostingChatComponent отсутствует на сущности с UID: " + uid);
            return;
        }

        _speakTimer.Interval = component.SpeakTimerRead; // 8000 миллисекунд = 8 секунд по умолчанию
        _speakTimer.Elapsed += (sender, eventArgs) =>
        {
            // Проверяем, что данные в компоненте были обновлены
            if (component.PostingMessageSpeak != null)
            {
                _chat.TrySendInGameICMessage(uid, component.PostingMessageSpeak, InGameICChatType.Speak, ChatTransmitRange.Normal);
            }
            _speakTimer.Interval = component.SpeakTimerRead;
        };
        _emoteTimer.Interval = component.EmoteTimerRead; // 9000 миллисекунд = 9 секунд по умолчанию
        _emoteTimer.Elapsed += (sender, eventArgs) =>
        {
            // Проверяем, что данные в компоненте были обновлены
            if (component.PostingMessageEmote != null)
            {
                _chat.TrySendInGameICMessage(uid, component.PostingMessageEmote, InGameICChatType.Emote, ChatTransmitRange.Normal);
            }
            _emoteTimer.Interval = component.EmoteTimerRead;
        };
        // Запускаем таймеры
        _speakTimer.Start();
        _emoteTimer.Start();
    }
}