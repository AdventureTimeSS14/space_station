/// <summary>
/// This system can be used for send emote-message to clients when some custom item was used.
///
/// by ModerN, mailto:modern-nm@yandex.by or https://github.com/modern-nm. Discord: modern.df
/// </summary>

using Content.Server.Chat.Systems;
using Content.Shared.ADT;
using Content.Shared.Interaction.Events;

namespace Content.Server.ADT;

public sealed class EmitEmoteSystem : EntitySystem {
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmitEmoteOnUseComponent, UseInHandEvent>(handler: OnEmitEmoteOnUseInHand);
    }

    private void OnEmitEmoteOnUseInHand(EntityUid uid, EmitEmoteOnUseComponent component, UseInHandEvent args)
    {
        // Intentionally not checking whether the interaction has already been handled.
        TryEmitEmote(uid, component, args.User);

        if (component.Handle)
            args.Handled = true;
    }

    private void TryEmitEmote(EntityUid uid, EmitEmoteOnUseComponent component, EntityUid? user = null, bool predict = true)
    {
        if (component.EmoteType == null)
            return;

        TryPrototype(uid, out var prototype);
        if (prototype != null)
            if (prototype.ID.ToLower() == "adtdumbbell")
            {
                EntityManager.TrySystem<ChatSystem>(out var chatSystem);
                if (chatSystem != null && user != null)
                {

                    chatSystem.TryEmoteWithChat((EntityUid) user, component.EmoteType); //Resources\Prototypes\Voice\speech_emotes.yml
                    //chatSystem.TrySendInGameICMessage((EntityUid) user, component.EmoteType, InGameICChatType.Emote, false);
                }
            }
    }
}
