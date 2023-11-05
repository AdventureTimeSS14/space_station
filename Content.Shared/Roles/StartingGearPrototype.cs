using Content.Shared.Preferences;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Humanoid; // Sirena-Underwear
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.Roles
{
    [Prototype("startingGear")]
    public sealed partial class StartingGearPrototype : IPrototype
    {
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
    }
}
