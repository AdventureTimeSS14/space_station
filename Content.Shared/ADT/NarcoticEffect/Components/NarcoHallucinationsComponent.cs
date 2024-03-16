using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Prototypes;

namespace Content.Shared.NarcoticEffects.Components;

[RegisterComponent]
public sealed partial class NarcoHallucinationsComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextSecond = TimeSpan.Zero;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Range = 7f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Chance = 0.5f;
    public List<EntProtoId> Spawns = new List<EntProtoId> { "ADTHallucinationMob1", "ADTHallucinationMob2" };

}
