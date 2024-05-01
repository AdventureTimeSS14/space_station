using Robust.Shared.Audio;
using Content.Shared.Polymorph;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Content.Shared.Actions;

namespace Content.Shared.Changeling.Components;

[RegisterComponent]
[AutoGenerateComponentState(true)]
public sealed partial class ChangelingComponent : Component
{
    /// <summary>
    /// The amount of chemicals the ling has.
    /// </summary>
    [DataField]
    public float Chemicals = 20f;

    [DataField]
    public float Accumulator = 0f;

    /// <summary>
    /// The amount of chemicals passively generated per second
    /// </summary>
    [DataField]
    public float ChemicalsPerSecond = 0.5f;

    /// <summary>
    /// The lings's current max amount of chemicals.
    /// </summary>
    [DataField]
    public float MaxChemicals = 75f;

    /// <summary>
    /// The maximum amount of DNA strands a ling can have at one time
    /// </summary>
    [DataField]
    public int DNAStrandCap = 7;

    /// <summary>
    /// List of stolen DNA
    /// </summary>
    [DataField]
    public List<PolymorphHumanoidData> StoredDNA = new List<PolymorphHumanoidData>();

    /// <summary>
    /// The DNA index that the changeling currently has selected
    /// </summary>
    [DataField]
    public int SelectedDNA = 0;

    /// </summary>
    /// Flesh sound
    /// </summary>
    public SoundSpecifier? SoundFlesh = new SoundPathSpecifier("/Audio/Effects/blobattack.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f),
    };

    /// </summary>
    /// Flesh sound
    /// </summary>
    public SoundSpecifier? SoundFleshQuiet = new SoundPathSpecifier("/Audio/Effects/blobattack.ogg")
    {
        Params = AudioParams.Default.WithVolume(-1f),
    };

    public SoundSpecifier? SoundResonant = new SoundPathSpecifier("/Audio/ADT/resonant.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f),
    };

    /// <summary>
    /// Blind sting duration
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan BlindStingDuration = TimeSpan.FromSeconds(18);

    /// <summary>
    /// Refresh ability
    /// </summary>
    public bool CanRefresh = false;

    #region Actions

    public EntProtoId ChangelingEvolutionMenuAction = "ActionChangelingEvolutionMenu";

    [AutoNetworkedField]
    public EntityUid? ChangelingEvolutionMenuActionEntity;

    public EntProtoId ChangelingRegenAction = "ActionLingRegenerate";

    [AutoNetworkedField]
    public EntityUid? ChangelingRegenActionEntity;

    public EntProtoId ChangelingAbsorbAction = "ActionChangelingAbsorb";

    [AutoNetworkedField]
    public EntityUid? ChangelingAbsorbActionEntity;

    public EntProtoId ChangelingDNACycleAction = "ActionChangelingCycleDNA";

    [AutoNetworkedField]
    public EntityUid? ChangelingDNACycleActionEntity;

    public EntProtoId ChangelingTransformAction = "ActionChangelingTransform";

    [AutoNetworkedField]
    public EntityUid? ChangelingTransformActionEntity;

    public EntProtoId ChangelingRefreshAction = "ActionLingRefresh";

    [AutoNetworkedField]
    public EntityUid? ChangelingRefreshActionEntity;

    public EntProtoId ChangelingDNAStingAction = "ActionLingStingExtract";

    [AutoNetworkedField]
    public EntityUid? ChangelingDNAStingActionEntity;

    public EntProtoId ChangelingLesserFormAction = "ActionLingLesserForm";

    [AutoNetworkedField]
    public EntityUid? ChangelingLesserFormActionEntity;

    public EntProtoId ChangelingLastResortAction = "ActionLingLastResort";

    [AutoNetworkedField]
    public EntityUid? ChangelingLastResortActionEntity;

    public EntProtoId ChangelingStasisDeathAction = "ActionStasisDeath";

    [AutoNetworkedField]
    public EntityUid? ChangelingStasisDeathActionEntity;

    #endregion

    #region Chemical Costs
    public float ChemicalsCostFree = 0;

    public float ChemicalsCostFive = -5f;

    public float ChemicalsCostTen = -10f;

    public float ChemicalsCostFifteen = -15f;

    public float ChemicalsCostTwenty = -20f;

    public float ChemicalsCostTwentyFive = -25f;

    public float ChemicalsCostFifty = -50f;
    #endregion

    #region DNA Absorb Ability
    /// <summary>
    /// How long an absorb stage takes, in seconds.
    /// </summary>
    [DataField]
    public int AbsorbDuration = 10;

    /// <summary>
    /// The stage of absorbing that the changeling is on. Maximum of 2 stages.
    /// </summary>
    [DataField]
    public int AbsorbStage = 0;

    /// <summary>
    /// The amount of genetic damage the target gains when they're absorbed.
    /// </summary>
    [DataField]
    public float AbsorbGeneticDmg = 200.0f;

    /// <summary>
    /// The amount of evolution points the changeling gains when they absorb another changeling.
    /// </summary>
    [DataField]
    public float AbsorbedChangelingPointsAmount = 5.0f;

    /// <summary>
    /// The amount of evolution points the changeling gains when they absorb somebody.
    /// </summary>
    [DataField]
    public float AbsorbedMobPointsAmount = 2.0f;

    [DataField]
    public float AbsorbedDnaModifier = 0f;

    #endregion

    #region Regenerate Ability
    /// <summary>
    /// The amount of burn damage is healed when the regenerate ability is sucesssfully used.
    /// </summary>
    [DataField]
    public float RegenerateBurnHealAmount = -100f;

    /// <summary>
    /// The amount of brute damage is healed when the regenerate ability is sucesssfully used.
    /// </summary>
    [DataField]
    public float RegenerateBruteHealAmount = -125f;

    /// <summary>
    /// The amount of blood volume that is gained when the regenerate ability is sucesssfully used.
    /// </summary>
    [DataField]
    public float RegenerateBloodVolumeHealAmount = 1000f;

    /// <summary>
    /// The amount of bleeding that is reduced when the regenerate ability is sucesssfully used.
    /// </summary>
    [DataField]
    public float RegenerateBleedReduceAmount = -1000f;

    /// <summary>
    /// Sound that plays when the ling uses the regenerate ability.
    /// </summary>
    [DataField]
    public SoundSpecifier? SoundRegenerate = new SoundPathSpecifier("/Audio/Effects/demon_consume.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f),
    };
    #endregion

    #region Armblade Ability
    /// <summary>
    /// If the ling has an active armblade or not.
    /// </summary>
    public bool ArmBladeActive = false;

    [AutoNetworkedField]
    public EntityUid? BladeEntity;

    #endregion

    #region Chitinous Armor Ability
    /// <summary>
    /// The amount of chemical regeneration is reduced when the ling armor is active.
    /// </summary>
    [DataField]
    public float LingArmorRegenCost = 0.125f;

    /// <summary>
    /// If the ling has the armor on or not.
    /// </summary>
    public bool LingArmorActive = false;
    #endregion

    #region Chameleon Skin Ability
    /// <summary>
    /// If the ling has chameleon skin active or not.
    /// </summary>
    public bool ChameleonSkinActive = false;

    /// <summary>
    /// How fast the changeling will turn invisible from standing still when using chameleon skin.
    /// </summary>
    [DataField]
    public float ChameleonSkinPassiveVisibilityRate = -0.10f;

    /// <summary>
    /// How fast the changeling will turn visible from movement when using chameleon skin.
    /// </summary>
    [DataField]
    public float ChameleonSkinMovementVisibilityRate = 0.60f;
    #endregion

    #region Dissonant Shriek Ability
    /// <summary>
    /// Range of the Dissonant Shriek's EMP in tiles.
    /// </summary>
    [DataField]
    public float DissonantShriekEmpRange = 5f;

    /// <summary>
    /// Power consumed from batteries by the Dissonant Shriek's EMP
    /// </summary>
    [DataField]
    public float DissonantShriekEmpConsumption = 50000f;

    /// <summary>
    /// How long the Dissonant Shriek's EMP effects last for
    /// </summary>
    [DataField]
    public float DissonantShriekEmpDuration = 12f;
    #endregion

    #region Stasis Death Ability

    public float StasisDeathDamageAmount = 10000f;    /// Damage gain to die

    public bool StasisDeathActive = false;      /// Is ling dead or alive

    #endregion

    #region Muscles Ability

    public bool MusclesActive = false;

    [DataField]
    public float MusclesModifier = 2f;

    [DataField]
    public float MusclesStaminaDamage = 3f;

    #endregion

    #region Changeling Chemicals

    [DataField("chemicalMorphine", customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string ChemicalMorphine = "ADTMMorphine";

    [DataField("chemicalMute", customTypeSerializer: typeof(PrototypeIdSerializer<ReagentPrototype>))]
    public string ChemicalMute = "MuteToxin";

    #endregion

    #region Changeling Chemicals Amount

    [ViewVariables(VVAccess.ReadWrite), DataField("adrenalineAmount")]
    public float AdrenalineAmount = 5f;

    [ViewVariables(VVAccess.ReadWrite), DataField("muteAmount")]
    public float MuteAmount = 20f;

    [ViewVariables(VVAccess.ReadWrite), DataField("drugAmount")]
    public float SpaceDrugsAmount = 50f;

    #endregion

    #region Lesser Form Ability

    public bool LesserFormActive = false;

    public string LesserFormMob = "ChangelingLesserForm";


    #endregion

    #region Armshield Ability
    /// <summary>
    /// If the ling has an active armblade or not.
    /// </summary>
    public bool ArmShieldActive = false;

    [AutoNetworkedField]
    public EntityUid? ShieldEntity;

    #endregion

    public float GibDamage = 5000f;

    public bool EggedBody = false;

    public bool EggsReady = false;

    [DataField]
    public int BiodegradeDuration = 3;
    
    public string HiveMind = "ChangelingCollectiveMind";

    public bool ShowName = false;

    public bool ShowRank = true;

    public string RankName = "collective-mind-ling-rank";
}
