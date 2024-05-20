using System.Linq;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Content.Shared.Radio.Components; // Parkstation-IPC
using Content.Shared.Containers; // Parkstation-IPC
using Robust.Shared.Containers; // Parkstation-IPC
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Collections;
using Robust.Shared.Prototypes;

namespace Content.Shared.Station;

public abstract class SharedStationSpawningSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] protected readonly InventorySystem InventorySystem = default!;
    [Dependency] private   readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private   readonly SharedStorageSystem _storage = default!;
    [Dependency] private   readonly SharedTransformSystem _xformSystem = default!;

    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<InventoryComponent> _inventoryQuery;
    private EntityQuery<StorageComponent> _storageQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        _handsQuery = GetEntityQuery<HandsComponent>();
        _inventoryQuery = GetEntityQuery<InventoryComponent>();
        _storageQuery = GetEntityQuery<StorageComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    /// <summary>
    ///     Equips the given starting gears from a `RoleLoadout` onto an entity.
    /// </summary>
    public void EquipRoleLoadout(EntityUid entity, RoleLoadout loadout, RoleLoadoutPrototype roleProto)
    {
        // Order loadout selections by the order they appear on the prototype.
        foreach (var group in loadout.SelectedLoadouts.OrderBy(x => roleProto.Groups.FindIndex(e => e == x.Key)))
        {
            foreach (var items in group.Value)
            {
                if (!PrototypeManager.TryIndex(items.Prototype, out var loadoutProto))
                {
                    Log.Error($"Unable to find loadout prototype for {items.Prototype}");
                    continue;
                }

                if (!PrototypeManager.TryIndex(loadoutProto.Equipment, out var startingGear))
                {
                    Log.Error($"Unable to find starting gear {loadoutProto.Equipment} for loadout {loadoutProto}");
                    continue;
                }

                // Handle any extra data here.
                EquipStartingGear(entity, startingGear, raiseEvent: false);
            }
        }
    }

    /// <summary>
    /// <see cref="EquipStartingGear(Robust.Shared.GameObjects.EntityUid,System.Nullable{Robust.Shared.Prototypes.ProtoId{Content.Shared.Roles.StartingGearPrototype}},bool)"/>
    /// </summary>
    public void EquipStartingGear(EntityUid entity, ProtoId<StartingGearPrototype>? startingGear, bool raiseEvent = true)
    {
        PrototypeManager.TryIndex(startingGear, out var gearProto);
        EquipStartingGear(entity, gearProto);
    }

    /// <summary>
    /// Equips starting gear onto the given entity.
    /// </summary>
    /// <param name="entity">Entity to load out.</param>
    /// <param name="startingGear">Starting gear to use.</param>
    /// <param name="raiseEvent">Should we raise the event for equipped. Set to false if you will call this manually</param>
    public void EquipStartingGear(EntityUid entity, StartingGearPrototype? startingGear, bool raiseEvent = true)
    {
        if (startingGear == null)
            return;

        var xform = _xformQuery.GetComponent(entity);

        if (InventorySystem.TryGetSlots(entity, out var slotDefinitions))
        {
            foreach (var slot in slotDefinitions)
            {
                var equipmentStr = startingGear.GetGear(slot.Name);
                if (!string.IsNullOrEmpty(equipmentStr))
                {
                    var equipmentEntity = EntityManager.SpawnEntity(equipmentStr, xform.Coordinates);
                    InventorySystem.TryEquip(entity, equipmentEntity, slot.Name, silent: true, force:true);
                }
            }
        }

        // Parkstation-Ipc-Start
        // This is kinda gross, and weird, and very hardcoded, but it's the best way I could think of to do it.
        // This is replicated in SetOutfitCommand.SetOutfit.
        // If they have an EncryptionKeyHolderComponent, spawn in their headset, find the
        // EncryptionKeyHolderComponent on it, move the keys over, and delete the headset.
        if (TryComp<EncryptionKeyHolderComponent>(entity, out var keyHolderComp))
        {
            var containerMan = EntityManager.System<SharedContainerSystem>();

            var earEquipString = startingGear.GetGear("ears", profile);

            if (!string.IsNullOrEmpty(earEquipString))
            {
                var earEntity = Spawn(earEquipString, Transform(entity).Coordinates);

                if (TryComp<EncryptionKeyHolderComponent>(earEntity, out _) && // I had initially wanted this to spawn the headset, and simply move all the keys over, but the headset didn't seem to have any keys in it when spawned...
                    TryComp<ContainerFillComponent>(earEntity, out var fillComp) &&
                    fillComp.Containers.TryGetValue(EncryptionKeyHolderComponent.KeyContainerName, out var defaultKeys))
                {
                    containerMan.CleanContainer(keyHolderComp.KeyContainer);

                    foreach (var key in defaultKeys)
                    {
                        var keyEntity = Spawn(key, Transform(entity).Coordinates);
                        //TODO:xTray тут что то сделал непонятно что
                        //keyHolderComp.KeyContainer.ContainedEntities.Append(keyEntity);
                        containerMan.Insert(keyEntity, keyHolderComp.KeyContainer);
                    }
                }

                EntityManager.QueueDeleteEntity(earEntity);
            }
        }
        // Parkstation-Ipc-End

        if (!TryComp(entity, out HandsComponent? handsComponent))
            return;

        var inhand = startingGear.Inhand;
        var coords = EntityManager.GetComponent<TransformComponent>(entity).Coordinates;
        if (_handsQuery.TryComp(entity, out var handsComponent))
        foreach (var prototype in inhand)
        {
            var inhand = startingGear.Inhand;
            var coords = xform.Coordinates;
            foreach (var prototype in inhand)
            {
                var inhandEntity = EntityManager.SpawnEntity(prototype, coords);

                if (_handsSystem.TryGetEmptyHand(entity, out var emptyHand, handsComponent))
                {
                    _handsSystem.TryPickup(entity, inhandEntity, emptyHand, checkActionBlocker: false, handsComp: handsComponent);
                }
            }
        }

        if (startingGear.Storage.Count > 0)
        {
            var coords = _xformSystem.GetMapCoordinates(entity);
            var ents = new ValueList<EntityUid>();
            _inventoryQuery.TryComp(entity, out var inventoryComp);

            foreach (var (slot, entProtos) in startingGear.Storage)
            {
                if (entProtos.Count == 0)
                    continue;

                foreach (var ent in entProtos)
                {
                    ents.Add(Spawn(ent, coords));
                }

                if (inventoryComp != null &&
                    InventorySystem.TryGetSlotEntity(entity, slot, out var slotEnt, inventoryComponent: inventoryComp) &&
                    _storageQuery.TryComp(slotEnt, out var storage))
                {
                    foreach (var ent in ents)
                    {
                        _storage.Insert(slotEnt.Value, ent, out _, storageComp: storage, playSound: false);
                    }
                }
            }
        }

        if (raiseEvent)
        {
            var ev = new StartingGearEquippedEvent(entity);
            RaiseLocalEvent(entity, ref ev, true);
        }
    }
}
