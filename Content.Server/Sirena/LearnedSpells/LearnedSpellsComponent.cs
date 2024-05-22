using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Sirena.LearnedSpells;

[RegisterComponent]
public sealed partial class LearnedSpellsComponent : Component
{
    // Я и без вас знаю что это пиздец ебучий. Найдёте способ сделать динам. массивом - пинганите (by NekoDar)
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string? Spell1;
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string? Spell2;
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string? Spell3;
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string? Spell4;
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string? Spell5;
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string? Spell6;
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string? Spell7;
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string? Spell8;
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string? Spell9;
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))] public string? Spell10;

    [DataField] public EntityUid? SpellContainer1;
    [DataField] public EntityUid? SpellContainer2;
    [DataField] public EntityUid? SpellContainer3;
    [DataField] public EntityUid? SpellContainer4;
    [DataField] public EntityUid? SpellContainer5;
    [DataField] public EntityUid? SpellContainer6;
    [DataField] public EntityUid? SpellContainer7;
    [DataField] public EntityUid? SpellContainer8;
    [DataField] public EntityUid? SpellContainer9;
    [DataField] public EntityUid? SpellContainer10;
}
