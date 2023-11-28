using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Content.Shared.Atmos.Miasma;


namespace Content.Server.Chemistry.ReagentEffects;

public sealed partial class Embalm : ReagentEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
      => Loc.GetString("reagent-effect-guidebook-embalm", ("chance", Probability));

    // Gives the entity a component that prevents rotting and also execution by defibrillator
    public override void Effect(ReagentEffectArgs args)
    {
        var entityManager = args.EntityManager;
        entityManager.EnsureComponent<EmbalmedComponent>(args.SolutionEntity);
    }
}
