using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Server.Hallucinations;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Hallucinations;
using Content.Shared.StatusEffect;

namespace Content.Server.Chemistry.ReagentEffects
{
    /// <summary>
    /// Default metabolism for stimulants and tranqs. Attempts to find a MovementSpeedModifier on the target,
    /// adding one if not there and to change the movespeed
    /// </summary>
    public sealed partial class HallucinationsReagentEffect : ReagentEffect
    {
        [DataField("key")]
        public string Key = "ADTHallucinations";

        [DataField(required: true)]
        public string Proto = String.Empty;

        [DataField]
        public float Time = 2.0f;

        [DataField]
        public bool Refresh = true;

        [DataField]
        public HallucinationsMetabolismType Type = HallucinationsMetabolismType.Add;

        protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        {
            return Loc.GetString("reagent-effect-guidebook-hallucinations",
                ("chance", Probability),
                ("time", Time));
        }

        public override void Effect(ReagentEffectArgs args)
        {
            var statusSys = args.EntityManager.EntitySysManager.GetEntitySystem<StatusEffectsSystem>();
            var hallucinationsSys = args.EntityManager.EntitySysManager.GetEntitySystem<HallucinationsSystem>();

            var time = Time;
            time *= args.Scale;

            if (Type == HallucinationsMetabolismType.Add)
            {
                if (!hallucinationsSys.StartHallucinations(args.SolutionEntity, Key, TimeSpan.FromSeconds(Time), Refresh, Proto))
                    return;
            }
            else if (Type == HallucinationsMetabolismType.Remove)
            {
                statusSys.TryRemoveTime(args.SolutionEntity, Key, TimeSpan.FromSeconds(time));
            }
            else if (Type == HallucinationsMetabolismType.Set)
            {
                statusSys.TrySetTime(args.SolutionEntity, Key, TimeSpan.FromSeconds(time));
            }
        }
    }
    public enum HallucinationsMetabolismType
    {
        Add,
        Remove,
        Set
    }
}
