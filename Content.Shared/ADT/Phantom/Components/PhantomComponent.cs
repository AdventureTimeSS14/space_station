using System.Numerics;
using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Phantom.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(true)]
public sealed partial class PhantomComponent : Component
{

    #region Actions

    [DataField]
    public EntProtoId PhantomHauntAction = "ActionPhantomHaunt";

    [DataField, AutoNetworkedField]
    public EntityUid? PhantomHauntActionEntity;

    [DataField]
    public EntProtoId PhantomStopHauntAction = "ActionPhantomStopHaunt";

    [DataField, AutoNetworkedField]
    public EntityUid? PhantomStopHauntActionEntity;

    [DataField]
    public EntProtoId PhantomMakeVesselAction = "ActionPhantomMakeVessel";

    [DataField, AutoNetworkedField]
    public EntityUid? PhantomMakeVesselActionEntity;

    [DataField]
    public EntProtoId PhantomCycleVesselsAction = "ActionPhantomCycleVessels";

    [DataField, AutoNetworkedField]
    public EntityUid? PhantomCycleVesselsActionEntity;

    [DataField]
    public EntProtoId PhantomHauntVesselAction = "ActionPhantomHauntVessel";

    [DataField, AutoNetworkedField]
    public EntityUid? PhantomHauntVesselActionEntity;

    [DataField]
    public EntProtoId PhantomParalysisAction = "ActionPhantomParalysis";

    [DataField, AutoNetworkedField]
    public EntityUid? PhantomParalysisActionEntity;

    #endregion

    #region Toggleable Actions

    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(30);

    [DataField]
    public bool ParalysisOn = false;

    #endregion

    /// <summary>
    /// The total amount of Essence the revenant has. Functions
    /// as health and is regenerated.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 Essence = 50;

    /// <summary>
    /// Prototype to spawn when the entity dies.
    /// </summary>
    [DataField("spawnOnDeathPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string SpawnOnDeathPrototype = "Ectoplasm";

    /// <summary>
    /// The entity's current max amount of essence. Can be increased
    /// through harvesting player souls.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("maxEssence")]
    public FixedPoint2 EssenceRegenCap = 75;

    /// <summary>
    /// The amount of essence passively generated per second.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("essencePerSecond")]
    public FixedPoint2 EssencePerSecond = 0.5f;

    [ViewVariables]
    public float Accumulator = 0;

    [DataField]
    public EntityUid Holder = new EntityUid();

    [DataField]
    public bool HasHaunted = false;

    [DataField]
    public List<EntityUid> Vessels = new List<EntityUid>();

    [DataField]
    public int SelectedVessel = 0;

    [DataField]
    public int VesselsStrandCap = 10;

    [DataField]
    public int MakeVesselDuration = 4;

    [DataField]
    public float HolyDamageMultiplier = 5f;

    [DataField]
    public bool IsCorporeal = false;

    #region Visualizer
    [DataField("state")]
    public string State = "phantom";
    [DataField("hauntState")]
    public string HauntingState = "haunt";
    #endregion

}
