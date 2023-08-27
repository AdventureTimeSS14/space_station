using Content.Shared.Chemistry.Reagent;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Robust.Shared.Prototypes;

namespace Content.Server.Sirena.Chemistry.ReagentEffectsCondition;

public sealed partial class SexCondition : ReagentEffectCondition
{
    [DataField("sex")]
    public Sex Sex = default!;

    [DataField("shouldHave")]
    public bool ShouldHave = true;

    public override bool Condition(ReagentEffectArgs args)
    {
        var entSex = args.EntityManager.GetComponent<HumanoidAppearanceComponent>(args.SolutionEntity).Sex;
        if (entSex == Sex)
            return ShouldHave;
        else
            return !ShouldHave;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return "Какое-то описание посмотрим в игре -- SexCondition";
    }
}
