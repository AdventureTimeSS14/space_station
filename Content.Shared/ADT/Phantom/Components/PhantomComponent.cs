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

    #endregion
    /// <summary>
    /// The total amount of Essence the revenant has. Functions
    /// as health and is regenerated.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float Essence = 75;

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
    public float EssenceRegenCap = 75;

    /// <summary>
    /// The amount of essence passively generated per second.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("essencePerSecond")]
    public float EssencePerSecond = 0.5f;

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
}
