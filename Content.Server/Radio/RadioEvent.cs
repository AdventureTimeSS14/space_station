using Content.Shared.Chat;
using Content.Shared.Language;
using Content.Shared.Radio;

namespace Content.Server.Radio;

[ByRefEvent]
<<<<<<< HEAD
public readonly record struct RadioReceiveEvent(
    // Frontier - languages mechanic
    EntityUid MessageSource,
    RadioChannelPrototype Channel,
    ChatMessage UnderstoodChatMsg,
    ChatMessage NotUnderstoodChatMsg,
    LanguagePrototype Language
);
=======
public readonly record struct RadioReceiveEvent(string Message, EntityUid MessageSource, RadioChannelPrototype Channel, EntityUid RadioSource, MsgChatMessage ChatMsg);

>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
/// <summary>
/// Use this event to cancel sending message per receiver
/// </summary>
[ByRefEvent]
public record struct RadioReceiveAttemptEvent(RadioChannelPrototype Channel, EntityUid RadioSource, EntityUid RadioReceiver)
{
    public readonly RadioChannelPrototype Channel = Channel;
    public readonly EntityUid RadioSource = RadioSource;
    public readonly EntityUid RadioReceiver = RadioReceiver;
    public bool Cancelled = false;
}

/// <summary>
/// Use this event to cancel sending message to every receiver
/// </summary>
[ByRefEvent]
public record struct RadioSendAttemptEvent(RadioChannelPrototype Channel, EntityUid RadioSource)
{
    public readonly RadioChannelPrototype Channel = Channel;
    public readonly EntityUid RadioSource = RadioSource;
    public bool Cancelled = false;
}
