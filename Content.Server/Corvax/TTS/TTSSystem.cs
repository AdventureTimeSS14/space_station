using System.Threading.Tasks;
using Content.Server.Chat.Systems;
using Content.Server.Language;
using Content.Shared.Corvax.CCCVars;
using Content.Shared.Corvax.TTS;
using Content.Shared.GameTicking;
using Content.Shared.Language;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server.Corvax.TTS;

// ReSharper disable once InconsistentNaming
public sealed partial class TTSSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TTSManager _ttsManager = default!;
    [Dependency] private readonly SharedTransformSystem _xforms = default!;
    [Dependency] private readonly LanguageSystem _language = default!;

    private const int MaxMessageChars = 100 * 2; // same as SingleBubbleCharLimit * 2
    private bool _isEnabled = false;

    public override void Initialize()
    {
        _cfg.OnValueChanged(CCCVars.TTSEnabled, v => _isEnabled = v, true);

        SubscribeLocalEvent<TransformSpeechEvent>(OnTransformSpeech);
        SubscribeLocalEvent<TTSComponent, EntitySpokeEvent>(OnEntitySpoke);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestartCleanup);

        SubscribeNetworkEvent<RequestGlobalTTSEvent>(OnRequestGlobalTTS);
    }

    private void OnRoundRestartCleanup(RoundRestartCleanupEvent ev)
    {
        _ttsManager.ResetCache();
    }

    private async void OnRequestGlobalTTS(RequestGlobalTTSEvent ev, EntitySessionEventArgs args)
    {
        if (!_isEnabled ||
            ev.Text.Length > MaxMessageChars ||
            !_prototypeManager.TryIndex<TTSVoicePrototype>(ev.VoiceId, out var protoVoice))
            return;

        var soundData = await GenerateTTS(ev.Text, protoVoice.Speaker);
        if (soundData is null)
            return;

        RaiseNetworkEvent(new PlayTTSEvent(soundData), Filter.SinglePlayer(args.SenderSession));
    }

    private async void OnEntitySpoke(EntityUid uid, TTSComponent component, EntitySpokeEvent args)
    {
        var voiceId = component.VoicePrototypeId;
        if (!_isEnabled ||
            args.Message.Length > MaxMessageChars ||
            voiceId == null)
            return;

        var voiceEv = new TransformSpeakerVoiceEvent(uid, voiceId);
        RaiseLocalEvent(uid, voiceEv);
        voiceId = voiceEv.VoiceId;

        if (!_prototypeManager.TryIndex<TTSVoicePrototype>(voiceId, out var protoVoice))
            return;

        if (args.ObfuscatedMessage != null)
        {
            HandleWhisper(uid, args.Message, args.ObfuscatedMessage, args.LanguageEncodedMessage, protoVoice.Speaker, args.Language);
            return;
        }

        HandleSay(uid, args.Message, args.LanguageEncodedMessage, protoVoice.Speaker, args.Language);
    }

    private async void HandleSay(EntityUid uid, string message, string encMessage, string speaker, LanguagePrototype language)
    {
        var soundData = await GenerateTTS(message, speaker);
        if (soundData is null) return;

        var encSoundData = await GenerateTTS(encMessage, speaker);
        if (encSoundData is null) return;

        var soundTtsEvent = new PlayTTSEvent(soundData, GetNetEntity(uid));
        var encSoundTtsEvent = new PlayTTSEvent(encSoundData, GetNetEntity(uid));

        var receptions = Filter.Pvs(uid).Recipients;

        foreach (var session in receptions)
        {
            if (!session.AttachedEntity.HasValue) continue;
            var canUnderstand = _language.CanUnderstand(session.AttachedEntity.Value, language);

            RaiseNetworkEvent(canUnderstand ? soundTtsEvent : encSoundTtsEvent,
                session);
        }
    }

    private async void HandleWhisper(EntityUid uid, string message, string obfMessage, string encMessage, string speaker, LanguagePrototype language)
    {

        var fullSoundData = await GenerateTTS(message, speaker, true);
        if (fullSoundData is null) return;

        var obfSoundData = await GenerateTTS(obfMessage, speaker, true);
        if (obfSoundData is null) return;

        var encSoundData = await GenerateTTS(encMessage, speaker, true);
        if (encSoundData is null) return;

        var fullTtsEvent = new PlayTTSEvent(fullSoundData, GetNetEntity(uid), true);
        var obfTtsEvent = new PlayTTSEvent(obfSoundData, GetNetEntity(uid), true);
        var encTtsEvent = new PlayTTSEvent(encSoundData, GetNetEntity(uid), true);

        // TODO: Check obstacles
        var xformQuery = GetEntityQuery<TransformComponent>();
        var sourcePos = _xforms.GetWorldPosition(xformQuery.GetComponent(uid), xformQuery);
        var receptions = Filter.Pvs(uid).Recipients;
        foreach (var session in receptions)
        {
            if (!session.AttachedEntity.HasValue) continue;
            var xform = xformQuery.GetComponent(session.AttachedEntity.Value);
            var distance = (sourcePos - _xforms.GetWorldPosition(xform, xformQuery)).Length();
            if (distance > ChatSystem.VoiceRange * ChatSystem.VoiceRange)
                continue;

            var canUnderstand = _language.CanUnderstand(session.AttachedEntity.Value, language);

            //TODO xTray Для тех кто далеко нужно сгенерировать отдельный звук с помехами
            RaiseNetworkEvent(distance > ChatSystem.WhisperClearRange ? canUnderstand ?  obfTtsEvent : encTtsEvent
                : canUnderstand ? fullTtsEvent : encTtsEvent,
                session);
        }
    }

    // ReSharper disable once InconsistentNaming
    private async Task<byte[]?> GenerateTTS(string text, string speaker, bool isWhisper = false)
    {
        var textSanitized = Sanitize(text);
        if (textSanitized == "") return null;
        if (char.IsLetter(textSanitized[^1]))
            textSanitized += ".";

        var ssmlTraits = SoundTraits.RateFast;
        if (isWhisper)
            ssmlTraits = SoundTraits.PitchVerylow;
        var textSsml = ToSsmlText(textSanitized, ssmlTraits);

        return await _ttsManager.ConvertTextToSpeech(speaker, textSsml);
    }
}

public sealed class TransformSpeakerVoiceEvent : EntityEventArgs
{
    public EntityUid Sender;
    public string VoiceId;

    public TransformSpeakerVoiceEvent(EntityUid sender, string voiceId)
    {
        Sender = sender;
        VoiceId = voiceId;
    }
}
