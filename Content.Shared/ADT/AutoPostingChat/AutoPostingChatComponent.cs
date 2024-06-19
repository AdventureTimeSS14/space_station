using Robust.Shared.Audio;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
namespace Content.Shared.ADT.AutoPostingChat;
[RegisterComponent]
[NetworkedComponent]
public sealed partial class AutoPostingChatComponent : Component
{
    /// <summary>
    ///Sets a random interval after each iteration of spoken messages
    /// </summary>
    [DataField("randomIntervalSpeak"), ViewVariables(VVAccess.ReadWrite)]
    public bool RandomIntervalSpeak = false;

    /// <summary>
    /// Sets a random interval after each iteration of spoken emotions
    /// </summary>
    [DataField("randomIntervalEmote"), ViewVariables(VVAccess.ReadWrite)]
    public bool RandomIntervalEmote = false;

    /// <summary>
    /// The interval in milliseconds between automatic speech messages.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("speakTimer")]
    public int SpeakTimerRead = 8000;

    /// <summary>
    /// The interval in milliseconds between automatic emote messages.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("emoteTimer")]
    public int EmoteTimerRead = 9000;

    /// <summary>
    /// The message that will be automatically spoken by the entity.
    /// </summary>
    [DataField("speakMessage")]
    public string? PostingMessageSpeak = "";

    /// <summary>
    /// The message that will be automatically emotes by the entity.
    /// </summary>
    [DataField("emoteMessage")]
    public string? PostingMessageEmote = "";
}
