using Content.Shared.Speech;
using Robust.Shared.Prototypes;

namespace Content.Server.VoiceMask;

[RegisterComponent]
public sealed partial class VoiceMaskerComponent : Component
{
<<<<<<< HEAD
    [ViewVariables(VVAccess.ReadWrite)] public string LastSetName = "Unknown";
    [ViewVariables(VVAccess.ReadWrite)] public string? LastSetVoice; // Corvax-TTS
=======
    [DataField]
    public string LastSetName = "Unknown";
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2

    [DataField]
    public ProtoId<SpeechVerbPrototype>? LastSpeechVerb;

    [DataField]
    public EntProtoId Action = "ActionChangeVoiceMask";

    [DataField]
    public EntityUid? ActionEntity;
}
