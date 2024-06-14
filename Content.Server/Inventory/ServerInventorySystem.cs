using Content.Shared.Explosion;
using Content.Shared.Inventory;

namespace Content.Server.Inventory
{
    public sealed class ServerInventorySystem : InventorySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<InventoryComponent, BeforeExplodeEvent>(OnExploded);
        }

        private void OnExploded(Entity<InventoryComponent> ent, ref BeforeExplodeEvent args)
        {
            // explode each item in their inventory too
            var slots = new InventorySlotEnumerator(ent);
            while (slots.MoveNext(out var slot))
            {
                if (slot.ContainedEntity != null)
                    args.Contents.Add(slot.ContainedEntity.Value);
            }
        }

<<<<<<< HEAD
        private void OnOpenSlotStorage(OpenSlotStorageNetworkMessage ev, EntitySessionEventArgs args)
        {
            if (args.SenderSession.AttachedEntity is not { Valid: true } uid)
                return;

            if (TryGetSlotEntity(uid, ev.Slot, out var entityUid) && TryComp<StorageComponent>(entityUid, out var storageComponent))
            {
                _storageSystem.OpenStorageUI(entityUid.Value, uid, storageComponent);
            }
        }

=======
>>>>>>> 24e7653c984da133283457da2089e629161a7ff2
        public void TransferEntityInventories(Entity<InventoryComponent?> source, Entity<InventoryComponent?> target)
        {
            if (!Resolve(source.Owner, ref source.Comp) || !Resolve(target.Owner, ref target.Comp))
                return;

            var enumerator = new InventorySlotEnumerator(source.Comp);
            while (enumerator.NextItem(out var item, out var slot))
            {
                if (!TryEquip(target, item, slot.Name, true, force: true))
                    TryUnequip(source, slot.Name, true, force: true);
            }
        }
    }
}
