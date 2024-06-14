using Content.Shared.Humanoid;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared.Preferences
{
    public interface ICharacterProfile
    {
        string Name { get; }

        ICharacterAppearance CharacterAppearance { get; }

        bool MemberwiseEquals(ICharacterProfile other);

        /// <summary>
        ///     Makes this profile valid so there's no bad data like negative ages.
        /// </summary>
<<<<<<< HEAD
        void EnsureValid(IConfigurationManager configManager, IPrototypeManager prototypeManager, string[] sponsorMarkings);
=======
        void EnsureValid(ICommonSession session, IDependencyCollection collection);
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2

        /// <summary>
        /// Gets a copy of this profile that has <see cref="EnsureValid"/> applied, i.e. no invalid data.
        /// </summary>
<<<<<<< HEAD
        ICharacterProfile Validated(IConfigurationManager configManager, IPrototypeManager prototypeManager, string[] sponsorMarkings);
=======
        ICharacterProfile Validated(ICommonSession session, IDependencyCollection collection);
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
    }
}
