using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Client.Buckle;
using Content.Client.Gravity;
using Content.Shared.ActionBlocker;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;

namespace Content.Client.Movement.Systems;

public sealed class ClientWaddleAnimationSystem : SharedWaddleAnimationSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly GravitySystem _gravity = default!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly BuckleSystem _buckle = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Start waddling
        SubscribeAllEvent<StartedWaddlingEvent>((msg, args) => StartWaddling(args.SenderSession.AttachedEntity));

        // Handle concluding animations
        SubscribeLocalEvent<WaddleAnimationComponent, AnimationCompletedEvent>(OnAnimationCompleted);

        // Stop waddling
        SubscribeAllEvent<StoppedWaddlingEvent>((msg, args) => StopWaddling(args.SenderSession.AttachedEntity));
    }

    private void StartWaddling(EntityUid? uid)
    {
        if (
            !EntityIsValid(uid, out var entity, out var component) ||
            _animation.HasRunningAnimation(entity.Value, component.KeyName) ||
            _gravity.IsWeightless(entity.Value) ||
            _buckle.IsBuckled(entity.Value) ||
            _mobState.IsIncapacitated(entity.Value) ||
            !TryComp<InputMoverComponent>(entity, out var mover) ||
            !_actionBlocker.CanMove(entity.Value, mover)
        )
            return;

        PlayWaddleAnimationUsing(entity.Value, component, CalculateAnimationLength(component, mover), CalculateTumbleIntensity(component));
    }

    private static float CalculateTumbleIntensity(WaddleAnimationComponent component)
    {
        return component.LastStep ? 360 - component.TumbleIntensity : component.TumbleIntensity;
    }

    private static float CalculateAnimationLength(WaddleAnimationComponent component, InputMoverComponent mover)
    {
        return mover.Sprinting ? component.AnimationLength * component.RunAnimationLengthMultiplier : component.AnimationLength;
    }

    private void OnAnimationCompleted(EntityUid uid, WaddleAnimationComponent component, AnimationCompletedEvent args)
    {
        if (args.Key != component.KeyName)
            return;

        if (!TryComp<InputMoverComponent>(uid, out var mover))
            return;

        PlayWaddleAnimationUsing(uid, component, CalculateAnimationLength(component, mover), CalculateTumbleIntensity(component));
    }

    private void StopWaddling(EntityUid? uid)
    {
        if (
            !EntityIsValid(uid, out var entity, out var component) ||
            !_animation.HasRunningAnimation(entity.Value, component.KeyName)
        )
            return;

        _animation.Stop(entity.Value, component.KeyName);

        if (!TryComp<SpriteComponent>(entity.Value, out var sprite))
            return;

        // Note that this is a hard-write to this sprite, not some layer-based operation. If this is called whilst a sprite
        // is lying down, it will make the sprite stand up, which usually looks wrong.
        sprite.Offset = new Vector2();
        sprite.Rotation = Angle.FromDegrees(0);
    }

    private bool EntityIsValid(EntityUid? uid, [NotNullWhen(true)] out EntityUid? entity, [NotNullWhen(true)] out WaddleAnimationComponent? component)
    {
        entity = null;
        component = null;

        if (!uid.HasValue)
            return false;

        entity = uid.Value;

        return TryComp(entity, out component);
    }

    private void PlayWaddleAnimationUsing(EntityUid uid, WaddleAnimationComponent component, float len, float tumbleIntensity)
    {
        component.LastStep = !component.LastStep;

        var anim = new Animation()
        {
            Length = TimeSpan.FromSeconds(len),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), 0),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(tumbleIntensity), len/2),
                        new AnimationTrackProperty.KeyFrame(Angle.FromDegrees(0), len/2),
                    }
                },
                new AnimationTrackComponentProperty()
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(new Vector2(), 0),
                        new AnimationTrackProperty.KeyFrame(component.HopIntensity, len/2),
                        new AnimationTrackProperty.KeyFrame(new Vector2(), len/2),
                    }
                }
            }
        };

        _animation.Play(uid, anim, component.KeyName);
    }
}
