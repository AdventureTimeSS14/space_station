using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Sirena.NightVision.Components;
using Content.Shared.Sirena.NightVision.Systems;
using Content.Shared.Sirena.PNV.Systems;


namespace Content.Shared.Sirena.PNV.Components;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class PNVComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string ActionProto = "NVToggleAction";
    [DataField] public EntityUid? ActionContainer;

    // TODO: Перенести зависимость цвета на ПНВ, а не существо
    //[DataField("color")]
    //public Color NightVisionColor = Color.Green;


}
