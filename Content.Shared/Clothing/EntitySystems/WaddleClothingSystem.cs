<<<<<<< HEAD
﻿using Content.Shared.Clothing.Components;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Components;
=======
﻿using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Inventory.Events;
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2

namespace Content.Shared.Clothing.EntitySystems;

public sealed class WaddleClothingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

<<<<<<< HEAD
        SubscribeLocalEvent<WaddleWhenWornComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<WaddleWhenWornComponent, GotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotEquipped(EntityUid entity, WaddleWhenWornComponent comp, GotEquippedEvent args)
    {
        var waddleAnimComp = EnsureComp<WaddleAnimationComponent>(args.Equipee);
=======
        SubscribeLocalEvent<WaddleWhenWornComponent, ClothingGotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<WaddleWhenWornComponent, ClothingGotUnequippedEvent>(OnGotUnequipped);
    }

    private void OnGotEquipped(EntityUid entity, WaddleWhenWornComponent comp, ClothingGotEquippedEvent args)
    {
        var waddleAnimComp = EnsureComp<WaddleAnimationComponent>(args.Wearer);
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2

        waddleAnimComp.AnimationLength = comp.AnimationLength;
        waddleAnimComp.HopIntensity = comp.HopIntensity;
        waddleAnimComp.RunAnimationLengthMultiplier = comp.RunAnimationLengthMultiplier;
        waddleAnimComp.TumbleIntensity = comp.TumbleIntensity;
    }

<<<<<<< HEAD
    private void OnGotUnequipped(EntityUid entity, WaddleWhenWornComponent comp, GotUnequippedEvent args)
    {
        RemComp<WaddleAnimationComponent>(args.Equipee);
=======
    private void OnGotUnequipped(EntityUid entity, WaddleWhenWornComponent comp, ClothingGotUnequippedEvent args)
    {
        RemComp<WaddleAnimationComponent>(args.Wearer);
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
    }
}
