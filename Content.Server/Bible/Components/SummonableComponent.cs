using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.FixedPoint;

namespace Content.Server.Bible.Components
{
    /// <summary>
    /// This lets you summon a mob or item with an alternative verb on the item
    /// </summary>
    [RegisterComponent]
    public sealed partial class SummonableComponent : Component
    {
        /// <summary>
        /// Used for a special item only the Chaplain can summon. Usually a mob, but supports regular items too.
        /// </summary>
        [DataField("specialItem", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string? SpecialItemPrototype = null;
        public bool AlreadySummoned = false;

        [DataField("requiresBibleUser")]
        public bool RequiresBibleUser = true;

        /// <summary>
        /// The specific creature this summoned, if the SpecialItemPrototype has a mobstate.
        /// </summary>
        [ViewVariables]
        public EntityUid? Summon = null;

        [ViewVariables]
        public EntityUid? PersonSummoned = null;

        [DataField("summonAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
        public string SummonAction = "ActionBibleSummon";

        [DataField("summonActionEntity")]
        public EntityUid? SummonActionEntity;

        /// Used for respawning
        [DataField("accumulator")]
        public float Accumulator = 0f;
        [DataField("respawnTime")]
        public float RespawnTime = 180f;

        /// <summary>
        /// How much energy will cost the summon
        /// </summary>
        [DataField("summonCost")]
        [ViewVariables(VVAccess.ReadWrite)]
        public FixedPoint2 SummonCost = 4;

        [ViewVariables(VVAccess.ReadWrite)]
        public bool Respawning = true;
    }
}
