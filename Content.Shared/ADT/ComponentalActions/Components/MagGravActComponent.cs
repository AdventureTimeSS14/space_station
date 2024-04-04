using Robust.Shared.Prototypes;

namespace Content.Shared.ComponentalActions.Components;

[RegisterComponent]
[AutoGenerateComponentState(true)]
public sealed partial class MagGravActComponent : Component
{
    // [DataField("blinkSound")]
    // public SoundSpecifier BlinkSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");

    // /// <summary>
    // /// Volume control for the spell.
    // /// </summary>
    // [DataField("blinkVolume")]
    // public float BlinkVolume = 5f;

    [DataField]
    public bool Active = false;

    [DataField("blinkAction")]
    public EntProtoId Action = "CompActionMagGrav";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;


}
