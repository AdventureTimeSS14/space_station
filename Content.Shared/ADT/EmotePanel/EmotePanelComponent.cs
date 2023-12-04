using Content.Shared.Actions;
using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
//
namespace Content.Shared.ADT.EmotePanel;

[RegisterComponent, NetworkedComponent]
public sealed partial class EmotePanelComponent: Component
{
    [DataField]
    public EntProtoId OpenEmotesAction = "ActionOpenEmotes";

    [DataField, AutoNetworkedField]
    public EntityUid? OpenEmotesActionEntity;
}
[Serializable, NetSerializable]
public sealed partial class RequestEmoteMenuEvent : EntityEventArgs
{
    public readonly List<string> Prototypes = new();
    public int Target { get; }

    public RequestEmoteMenuEvent(int target)
    {
        Target = target;
    }
}
[Serializable, NetSerializable]
public sealed partial class SelectEmoteEvent : EntityEventArgs
{
    public string PrototypeId { get; }
    public int Target { get; }

    public SelectEmoteEvent(int target, string prototypeId)
    {
        Target = target;
        PrototypeId = prototypeId;
    }
}
public sealed partial class OpenEmotesActionEvent : InstantActionEvent
{
}
