using Content.Shared.VoiceMask;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.VoiceMask;

public sealed class VoiceMaskBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _proto = default!;

    [ViewVariables]
    private VoiceMaskNameChangeWindow? _window;

    public VoiceMaskBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = new(_proto);

        _window.OpenCentered();
        _window.OnNameChange += OnNameSelected;
<<<<<<< HEAD
        _window.OnVoiceChange += (value) => SendMessage(new VoiceMaskChangeVoiceMessage(value)); // Corvax-TTS
=======
        _window.OnVerbChange += verb => SendMessage(new VoiceMaskChangeVerbMessage(verb));
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
        _window.OnClose += Close;
    }

    private void OnNameSelected(string name)
    {
        SendMessage(new VoiceMaskChangeNameMessage(name));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not VoiceMaskBuiState cast || _window == null)
        {
            return;
        }

<<<<<<< HEAD
        _window.UpdateState(cast.Name, cast.Voice); // Corvax-TTS
=======
        _window.UpdateState(cast.Name, cast.Verb);
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        _window?.Close();
    }
}
