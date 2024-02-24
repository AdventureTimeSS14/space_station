using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Anomaly.Effects.Components;

[RegisterComponent]
public sealed partial class OldTVAnomalyComponent : Component
{
    /// <summary>
    /// The maximum distance from which you can be blind by the anomaly.
    /// </summary>
    [DataField("maximumStaticRadius")]
    public float MaximumStaticRadius = 7f;

    [DataField("supercriticalStaticRadius")]
    public float CritStaticRadius = 20f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MaxStaticRange = 7f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float MaxStaticDamage = 20f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan MaxPassiveStaticDuration = TimeSpan.FromSeconds(4);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan MaxPulseStaticDuration = TimeSpan.FromSeconds(10);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan MaxSupercritStaticDuration = TimeSpan.FromSeconds(20);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float PassiveStaticChance = 0.7f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextSecond = TimeSpan.Zero;

}


[ByRefEvent]
public readonly record struct TVAnomalyPulseEvent(EntityUid Anomaly, float Stability, float Severity, TimeSpan Duration);

[ByRefEvent]
public readonly record struct TVAnomalySupercriticalEvent(EntityUid Anomaly, TimeSpan Duration);

