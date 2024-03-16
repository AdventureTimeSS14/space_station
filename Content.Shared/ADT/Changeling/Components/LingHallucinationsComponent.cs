using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;

namespace Content.Shared.Changeling.Components;

[RegisterComponent]
public sealed partial class LingHallucinationsComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextSecond = TimeSpan.Zero;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Range = 7f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float SpawnRate = 15f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Chance = 0.65f;
    public List<EntProtoId> Spawns = new List<EntProtoId> { "ADTHallucinationMobSpider", "ADTHallucinationMobSlime", "ADTHallucinationMobBehonker" };

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/ADT/ling-drugs.ogg")
    {
    };
}
