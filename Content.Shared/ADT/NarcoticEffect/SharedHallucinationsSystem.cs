//using Content.Shared.StatusEffect;
//using Content.Shared.NarcoticEffects.Components;
//using Content.Shared.Eye;
//
//namespace Content.Shared.Chemistry.EntitySystems;
//
//public abstract class SharedNarcoHallucinationsSystem : EntitySystem
//{
//
//    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
//    [Dependency] private readonly IEntityManager _entityManager = default!;
//    [Dependency] private readonly SharedEyeSystem _eye = default!;
//
//
//    public override void Initialize()
//    {
//        base.Initialize();
//
//        SubscribeLocalEvent<NarcoHallucinationsComponent, ComponentInit>(OnInit);
//        SubscribeLocalEvent<NarcoHallucinationsComponent, ComponentShutdown>(OnShutdown);
//
//    }
//
//    private void OnInit(EntityUid uid, NarcoHallucinationsComponent component, ComponentInit args)
//    {
//        if (!_entityManager.TryGetComponent<EyeComponent>(uid, out var eye))
//            return;
//        _eye.SetVisibilityMask(uid, eye.VisibilityMask | (int) VisibilityFlags.Narcotic, eye);
//    }
//
//    private void OnShutdown(EntityUid uid, NarcoHallucinationsComponent component, ComponentShutdown args)
//    {
//        if (!_entityManager.TryGetComponent<EyeComponent>(uid, out var eye))
//            return;
//        _eye.SetVisibilityMask(uid, eye.VisibilityMask & ~(int) VisibilityFlags.Narcotic, eye);
//    }
//}
