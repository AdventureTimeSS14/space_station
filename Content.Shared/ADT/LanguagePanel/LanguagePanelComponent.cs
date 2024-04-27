/// Made for Adventure Time Project by ModerN. https://github.com/modern-nm mailto:modern-nm@yandex.by
/// see also https://github.com/DocNITE/liebendorf-station/tree/feature/emote-radial-panel
using Content.Shared.Actions;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.ADT.LanguagePanel;


/// <summary>
/// This component describes ActionEntity "ActionOpenEmotes". This class is a part of code which is responsible for using RadialUiController.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class LanguagePanelComponent: Component
{
    [DataField]
    public EntProtoId OpenLanguagesAction = "ActionOpenLanguagesM";

    [DataField]
    public EntityUid? OpenLanguagesActionEntity;
}
/// <summary>
/// This event carries list of emotes-prototypes and entity - the source of request. This class is a part of code which is responsible for using RadialUiController.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class RequestLanguageMenuEvent : EntityEventArgs
{
    public readonly List<string> Languages = new();
    public int Target { get; }

    public RequestLanguageMenuEvent(int target)
    {
        Target = target;
    }
}

/// <summary>
/// This event carries prototype-id of emote, which was selected. This class is a part of code which is responsible for using RadialUiController.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class SelectLanguageEvent : EntityEventArgs
{
    public string PrototypeId { get; }
    public int Target { get; }

    public SelectLanguageEvent(int target, string prototypeId)
    {
        Target = target;
        PrototypeId = prototypeId;
    }
}
public sealed partial class OpenLanguagesActionEvent : InstantActionEvent
{
}
