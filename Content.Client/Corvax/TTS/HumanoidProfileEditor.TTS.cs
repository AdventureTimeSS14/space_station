using System.Linq;
using Content.Client.Corvax.Sponsors;
using Content.Client.Corvax.TTS;
using Content.Shared.Corvax.TTS;
using Content.Shared.Preferences;
using Robust.Shared.Random;
using Robust.Shared.Prototypes;

namespace Content.Client.Preferences.UI;

public sealed partial class HumanoidProfileEditor
{
    private TTSSystem _ttsSys = default!;
    private List<TTSVoicePrototype> _voiceList = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    private readonly List<string> _sampleText = new()
    {
        "Съешь же ещё этих мягких французских булок, да выпей чаю.",
        "Клоун, прекрати разбрасывать банановые кожурки офицерам под ноги!",
        "Капитан, вы уверены что хотите назначить клоуна на должность главы персонала?",
        "Эс Бэ! Тут человек в сером костюме, с тулбоксом и в маске! Помогите!!"
    };

    private void InitializeVoice()
    {
        _ttsSys = _entMan.System<TTSSystem>();
        _voiceList = _prototypeManager
            .EnumeratePrototypes<TTSVoicePrototype>()
            .Where(o => o.RoundStart)
            .OrderBy(o => Loc.GetString(o.Name))
            .ToList();

    }

    private void UpdateTTSVoicesControls()
    {
    }

    private void PlayTTS()
    {
    }
}
