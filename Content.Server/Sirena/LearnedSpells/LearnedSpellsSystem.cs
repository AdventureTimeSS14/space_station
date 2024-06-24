using Content.Shared.Actions;

namespace Content.Server.Sirena.LearnedSpells;

public sealed class LearnedSpellsSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LearnedSpellsComponent, ComponentInit>(OnComponentInit);
    }

    private void OnComponentInit(EntityUid uid, LearnedSpellsComponent component, ComponentInit args)
    {
        // Я и без вас знаю что это пиздец ебучий. Найдёте способ сделать динам. массивом - пинганите (by NekoDar)
        if (component.Spell1 != null)
            _actionsSystem.AddAction(uid, ref component.SpellContainer1, component.Spell1);

        if (component.Spell2 != null)
            _actionsSystem.AddAction(uid, ref component.SpellContainer2, component.Spell2);

        if (component.Spell3 != null)
            _actionsSystem.AddAction(uid, ref component.SpellContainer3, component.Spell3);

        if (component.Spell4 != null)
            _actionsSystem.AddAction(uid, ref component.SpellContainer4, component.Spell4);

        if (component.Spell5 != null)
            _actionsSystem.AddAction(uid, ref component.SpellContainer5, component.Spell5);

        if (component.Spell6 != null)
            _actionsSystem.AddAction(uid, ref component.SpellContainer6, component.Spell6);

        if (component.Spell7 != null)
            _actionsSystem.AddAction(uid, ref component.SpellContainer7, component.Spell7);

        if (component.Spell8 != null)
            _actionsSystem.AddAction(uid, ref component.SpellContainer8, component.Spell8);

        if (component.Spell9 != null)
            _actionsSystem.AddAction(uid, ref component.SpellContainer9, component.Spell9);

        if (component.Spell10 != null)
            _actionsSystem.AddAction(uid, ref component.SpellContainer10, component.Spell10);
    }
}
