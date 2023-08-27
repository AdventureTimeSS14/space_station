using Content.Server.Chemistry.ReactionEffects;
using Content.Server.Database;
using Content.Server.DetailExaminable;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Sirena;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Robust.Shared.Prototypes;

namespace Content.Server.Sirena.Chemistry.ReagentEffectsCondition;

public sealed partial class ERPStatusCondition : ReagentEffectCondition
{
    [DataField("erp")]
    public EnumERPStatus ERP = default!;

    [DataField("shouldHave")]
    public bool ShouldHave = true;

    public override bool Condition(ReagentEffectArgs args)
    {

        EnumERPStatus enterp;
        var hasCom = args.EntityManager.HasComponent<DetailExaminableComponent>(args.SolutionEntity);
        if (hasCom == true)
        enterp = args.EntityManager.GetComponent<DetailExaminableComponent>(args.SolutionEntity).ERPStatus;
        else
            return false;
        if (enterp == ERP)
            return ShouldHave;
        else
            return !ShouldHave;

    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return "Какое-то описание посмотрим в игре -- ERPStatusCondition";
    }
}
