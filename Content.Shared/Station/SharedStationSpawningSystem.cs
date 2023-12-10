using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Content.Shared.Radio.Components; // Parkstation-IPC
using Content.Shared.Containers; // Parkstation-IPC
using Robust.Shared.Containers; // Parkstation-IPC
using Content.Shared.Roles;

namespace Content.Shared.Station;

public abstract class SharedStationSpawningSystem : EntitySystem
{
    [Dependency] protected readonly InventorySystem InventorySystem = default!;
    [Dependency] private   readonly SharedHandsSystem _handsSystem = default!;

    /// <summary>
    /// Equips starting gear onto the given entity.
    /// </summary>
    /// <param name="entity">Entity to load out.</param>
    /// <param name="startingGear">Starting gear to use.</param>
    /// <param name="profile">Character profile to use, if any.</param>
    public void EquipStartingGear(EntityUid entity, StartingGearPrototype startingGear, HumanoidCharacterProfile? profile)
    {
        if (InventorySystem.TryGetSlots(entity, out var slotDefinitions))
        {
            foreach (var slot in slotDefinitions)
            {
                var equipmentStr = startingGear.GetGear(slot.Name, profile);
                if (!string.IsNullOrEmpty(equipmentStr))
                {
                    var equipmentEntity = EntityManager.SpawnEntity(equipmentStr, EntityManager.GetComponent<TransformComponent>(entity).Coordinates);
                    InventorySystem.TryEquip(entity, equipmentEntity, slot.Name, true, force:true);
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
                        keyHolderComp.KeyContainer.Insert(keyEntity, force: true);
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
        foreach (var prototype in inhand)
        {
            var inhandEntity = EntityManager.SpawnEntity(prototype, coords);

            if (_handsSystem.TryGetEmptyHand(entity, out var emptyHand, handsComponent))
            {
                _handsSystem.TryPickup(entity, inhandEntity, emptyHand, checkActionBlocker: false, handsComp: handsComponent);
            }
        }
    }
}
