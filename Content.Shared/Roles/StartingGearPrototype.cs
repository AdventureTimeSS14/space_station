using Robust.Shared.Prototypes;
<<<<<<< HEAD
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Humanoid; // Sirena-Underwear
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
=======
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2

namespace Content.Shared.Roles;

[Prototype]
public sealed partial class StartingGearPrototype : IPrototype, IInheritingPrototype
{
    /// <inheritdoc/>
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<StartingGearPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc/>
    [AbstractDataField]
    public bool Abstract { get; }

    /// <summary>
    /// The slot and entity prototype ID of the equipment that is to be spawned and equipped onto the entity.
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public Dictionary<string, EntProtoId> Equipment = new();

    /// <summary>
    /// The inhand items that are equipped when this starting gear is equipped onto an entity.
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public List<EntProtoId> Inhand = new(0);

    /// <summary>
    /// Inserts entities into the specified slot's storage (if it does have storage).
    /// </summary>
    [DataField]
    [AlwaysPushInheritance]
    public Dictionary<string, List<EntProtoId>> Storage = new();

    /// <summary>
    /// Gets the entity prototype ID of a slot in this starting gear.
    /// </summary>
    public string GetGear(string slot)
    {
<<<<<<< HEAD
        [DataField]
        public Dictionary<string, EntProtoId> Equipment = new();

        /// <summary>
        /// if empty, there is no skirt override - instead the uniform provided in equipment is added.
        /// </summary>
        [DataField]
        public EntProtoId? InnerClothingSkirt;

        [DataField]
        public EntProtoId? Satchel;

        [DataField]
        public EntProtoId? Duffelbag;

        [DataField]
        public List<EntProtoId> Inhand = new(0);

        // Sirena-Underwear-Start
        [DataField("underweart")]
        private string _underweart = string.Empty;

        [DataField("underwearb")]
        private string _underwearb = string.Empty;
        // Sirena-Underwear-End

        [ViewVariables]
        [IdDataField]
        public string ID { get; private set; } = string.Empty;

        public string GetGear(string slot, HumanoidCharacterProfile? profile)
        {
            if (profile != null)
            {
                if (slot == "jumpsuit" && profile.Clothing == ClothingPreference.Jumpskirt && !string.IsNullOrEmpty(InnerClothingSkirt))
                    return InnerClothingSkirt;
                if (slot == "back" && profile.Backpack == BackpackPreference.Satchel && !string.IsNullOrEmpty(Satchel))
                    return Satchel;
                if (slot == "back" && profile.Backpack == BackpackPreference.Duffelbag && !string.IsNullOrEmpty(Duffelbag))
                    return Duffelbag;
                // Sirena-Underwear-Start
                if (slot == "underweart" && profile.Sex == Sex.Female && !string.IsNullOrEmpty(_underweart))
                    return _underweart;
                if (slot == "underwearb" && profile.Sex == Sex.Female && !string.IsNullOrEmpty(_underwearb))
                    return _underwearb;
                // Sirena-Underwear-End
            }

            return Equipment.TryGetValue(slot, out var equipment) ? equipment : string.Empty;
        }
=======
        return Equipment.TryGetValue(slot, out var equipment) ? equipment : string.Empty;
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
    }
}
