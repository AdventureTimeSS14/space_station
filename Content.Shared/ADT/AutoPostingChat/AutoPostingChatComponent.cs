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
    /// Whether this destination is shown in the gateway ui.
    /// If you are making a gateway for an admeme set this once you are ready for players to select it.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool RandomIntervalSpeak = false;

    /// <summary>
    /// Whether this destination is shown in the gateway ui.
    /// If you are making a gateway for an admeme set this once you are ready for players to select it.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool RandomIntervalEmote = false;

    /// <summary>
    /// The interval in milliseconds between automatic speech messages.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public int SpeakTimerRead = 8000;

    /// <summary>
    /// The interval in milliseconds between automatic emote messages.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
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
