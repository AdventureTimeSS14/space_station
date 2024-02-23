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
}
