using System.Numerics;
<<<<<<< HEAD
=======
using Robust.Shared.GameStates;
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Defines something as causing waddling when worn.
/// </summary>
<<<<<<< HEAD
[RegisterComponent]
=======
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
public sealed partial class WaddleWhenWornComponent : Component
{
    ///<summary>
    /// How high should they hop during the waddle? Higher hop = more energy.
    /// </summary>
<<<<<<< HEAD
    [DataField]
=======
    [DataField, AutoNetworkedField]
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
    public Vector2 HopIntensity = new(0, 0.25f);

    /// <summary>
    /// How far should they rock backward and forward during the waddle?
    /// Each step will alternate between this being a positive and negative rotation. More rock = more scary.
    /// </summary>
<<<<<<< HEAD
    [DataField]
=======
    [DataField, AutoNetworkedField]
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
    public float TumbleIntensity = 20.0f;

    /// <summary>
    /// How long should a complete step take? Less time = more chaos.
    /// </summary>
<<<<<<< HEAD
    [DataField]
=======
    [DataField, AutoNetworkedField]
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
    public float AnimationLength = 0.66f;

    /// <summary>
    /// How much shorter should the animation be when running?
    /// </summary>
<<<<<<< HEAD
    [DataField]
=======
    [DataField, AutoNetworkedField]
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
    public float RunAnimationLengthMultiplier = 0.568f;
}
