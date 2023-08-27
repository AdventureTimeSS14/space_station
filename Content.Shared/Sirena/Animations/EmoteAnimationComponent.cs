using Content.Shared.Actions;
using Content.Shared.Actions.ActionTypes;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Sirena.Animations;

/// <summary>
/// </summary>
public partial class EmoteFlipActionEvent : InstantActionEvent { };

/// <summary>
/// </summary>
public partial class EmoteJumpActionEvent : InstantActionEvent { };

/// <summary>
/// </summary>
public partial class EmoteTurnActionEvent : InstantActionEvent { };

public partial class EmoteStopTailActionEvent : InstantActionEvent { };
public partial class EmoteStartTailActionEvent : InstantActionEvent { };

[RegisterComponent]
[NetworkedComponent]
public partial class EmoteAnimationComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public string AnimationId = "none";

    public InstantAction? FlipAction;
    public InstantAction? JumpAction;
    public InstantAction? TurnAction;
    public InstantAction? StopTailAction;
    public InstantAction? StartTailAction;

    [Serializable, NetSerializable]
    public class EmoteAnimationComponentState : ComponentState
    {
        public string AnimationId { get; init; }

        public EmoteAnimationComponentState(string animationId)
        {
            AnimationId = animationId;
        }
    }
}
