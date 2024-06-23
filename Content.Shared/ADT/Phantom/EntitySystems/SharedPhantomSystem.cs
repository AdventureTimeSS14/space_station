using Content.Shared.Actions;
using Content.Shared.Phantom.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Phantom;

public abstract class SharedPhantomSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Select phantom style (abilities pack)
    /// </summary>
    /// <param name="uid">Phantom uid</param>
    /// <param name="component">Phantom component</param>
    /// <param name="style">Style protoId</param>
    /// <param name="force">Force or not</param>
    public void SelectStyle(EntityUid uid, PhantomComponent component, string style, bool force = false)
    {
        if (!_proto.TryIndex<PhantomStylePrototype>(style, out var proto))
            return;
        if (style == component.CurrentStyle && !force)
            return;
        if (!force)
        {
            foreach (var action in component.CurrentActions)
            {
                _action.RemoveAction(uid, action);
                if (action != null)
                    QueueDel(action.Value);
            }
            component.CurrentActions.Clear();
        }
        var level = component.Vessels.Count;
        if (level <= 0)
            return;
        if (level > 0)
            AddFromList(uid, component, proto.Lvl1Actions);
        if (level > 1)
            AddFromList(uid, component, proto.Lvl2Actions);
        if (level > 2)
            AddFromList(uid, component, proto.Lvl3Actions);
        if (level > 3)
            AddFromList(uid, component, proto.Lvl4Actions);
        if (level > 4)
            AddFromList(uid, component, proto.Lvl5Actions);
        component.CurrentStyle = style;
    }

    /// <summary>
    /// Adding all actions from list
    /// </summary>
    /// <param name="uid">Phantom uid</param>
    /// <param name="component">Phantom component</param>
    /// <param name="list">Actions protoId list</param>
    public void AddFromList(EntityUid uid, PhantomComponent component, List<string> list)
    {
        foreach (var action in list)
        {
            if (action == null)
                continue;
            var actionEntity = new EntityUid?();
            _action.AddAction(uid, ref actionEntity, action);
            component.CurrentActions.Add(actionEntity);
            if (actionEntity != null)
                DirtyEntity(actionEntity.Value);
            DirtyEntity(uid);
            
        }
    }
}
