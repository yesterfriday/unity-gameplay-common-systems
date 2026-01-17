using System;
using System.Collections.Generic;

namespace Yesterfriday.GameplayCommonSystems.Inventory
{
    public sealed class InventoryModel
    {
       private readonly InventoryGrid _grid;

       public InventoryModel(int width, int height)
       {
           _grid = new InventoryGrid(width, height);
       }
       
       public event Action<IReadOnlyList<int>> OnInventoryChanged;
       
       public InventoryModel() : this(6, 6) { }
       
       //Demo
       public int SlotCount => _grid.SlotCount;

       public ItemStack GetSlot(int index) => _grid.GetSlot(index);

       
       //Query
       public int GetCount(ItemDefinition item)
       {
           if (item == null) return 0;

           int total = 0;
           for (int i = 0; i < _grid.SlotCount; i++)
           {
               var s = _grid.GetSlot(i);
               if (!s.IsEmpty && ReferenceEquals(s.Item, item))
               {
                   total += s.Amount;
               }
           }

           return total;
       }
       
       //Add
       public bool TryAdd(ItemDefinition item, int requested, out int added)
       {
           added = 0;
           if (item == null) return false;
           if (requested <= 0) return false;

           int remaining = requested;
           var changed = new HashSet<int>();

           // Pass 1: fill existing stacks
           for (int i = 0; i < _grid.SlotCount && remaining > 0; i++)
           {
               var slot = _grid.GetSlot(i);
               if (slot.IsEmpty) continue;
               if (!ReferenceEquals(slot.Item, item)) continue;

               int maxStack = item.MaxStack;
               int canAdd = maxStack - slot.Amount;
               if (canAdd <= 0) continue;

               int take = Math.Min(canAdd, remaining);
               slot.Add(take);
               _grid.SetSlot(i, slot);

               remaining -= take;
               changed.Add(i);
           }

           // Pass 2: create new stacks in empty slots
           for (int i = 0; i < _grid.SlotCount && remaining > 0; i++)
           {
               if (!_grid.IsSlotEmpty(i))
               {
                   continue;
               }

               int take = Math.Min(item.MaxStack, remaining);
               var newStack = new ItemStack(item, take);
               _grid.SetSlot(i, newStack);

               remaining -= take;
               changed.Add(i);
           }

           added = requested - remaining;
           bool changedAny = added > 0;

           if (changedAny)
           {
               NotifyChanged(changed);
           }

           return changedAny;
       }
       
       //Remove
       public bool TryRemove(ItemDefinition item, int requested, out int removed)
       {
           removed = 0;
           if (item == null) return false;
           if (requested <= 0) return false;

           int remaining = requested;
           var changed = new HashSet<int>();

           for (int i = 0; i < _grid.SlotCount && remaining > 0; i++)
           {
               var slot = _grid.GetSlot(i);
               if (slot.IsEmpty) continue;
               if (!ReferenceEquals(slot.Item, item)) continue;

               int take = Math.Min(slot.Amount, remaining);
               slot.Remove(take);
               _grid.SetSlot(i, slot);

               remaining -= take;
               changed.Add(i);
           }

           removed = requested - remaining;
           bool changedAny = removed > 0;

           if (changedAny)
               NotifyChanged(changed);

           return changedAny;
       }

       //Move
       public bool TryMove(int from, int to, int amount)
        {
            // Failure conditions
            if (from == to) return false;
            if (!_grid.IsValidIndex(from) || !_grid.IsValidIndex(to)) return false;

            var source = _grid.GetSlot(from);
            if (source.IsEmpty) return false;

            if (amount <= 0) return false;
            if (amount > source.Amount) return false;

            var changed = new HashSet<int>();

            // Target empty -> move amount
            if (_grid.IsSlotEmpty(to))
            {
                bool changedAny = MoveToEmpty(from, to, amount, ref source, changed);
                if (changedAny) NotifyChanged(changed);
                return changedAny;
            }

            var target = _grid.GetSlot(to);

            // Same item -> merge (partial allowed)
            if (!target.IsEmpty && ReferenceEquals(target.Item, source.Item))
            {
                bool changedAny = MergeSameItem(from, to, amount, ref source, ref target, changed);
                if (changedAny) NotifyChanged(changed);
                return changedAny;
            }

            // Different item -> swap full stacks, amount ignored
            bool swapped = SwapFull(from, to, source, target, changed);
            if (swapped) NotifyChanged(changed);
            return swapped;
        }
       
        // Internal helpers
        private bool MoveToEmpty(int from, int to, int amount, ref ItemStack source, HashSet<int> changed)
        {
            // amount validated (<= source.Amount)
            var moved = new ItemStack(source.Item, amount);
            _grid.SetSlot(to, moved);
            changed.Add(to);

            if (amount == source.Amount)
            {
                source.Clear();
            }
            else
            {
                source.Remove(amount);
            }

            _grid.SetSlot(from, source);
            changed.Add(from);

            return true;
        }

        private bool MergeSameItem(int from, int to, int amount, ref ItemStack source, ref ItemStack target, HashSet<int> changed)
        {
            int maxStack = source.Item.MaxStack;
            int capacity = maxStack - target.Amount;
            if (capacity <= 0) return false; // cannot merge, no change

            int take = Math.Min(capacity, amount);

            // Apply merge
            target.Add(take);
            _grid.SetSlot(to, target);
            changed.Add(to);

            // Reduce source by the moved amount (remainder stays)
            if (take == source.Amount)
            {
                source.Clear();
            }
            else
            {
                source.Remove(take);
            }

            _grid.SetSlot(from, source);
            changed.Add(from);

            return true;
        }

        private bool SwapFull(int from, int to, ItemStack source, ItemStack target, HashSet<int> changed)
        {
            // "Swap not possible" in v0.1 is effectively only invalid index/from==to (already handled).
            // Here we swap regardless of amount.
            _grid.SetSlot(from, target);
            _grid.SetSlot(to, source);
            changed.Add(from);
            changed.Add(to);
            return true;
        }

        private void NotifyChanged(HashSet<int> changed)
        {
            if (changed == null || changed.Count == 0) return;

            // Stable order is helpful for UI and debugging
            var list = new List<int>(changed);
            list.Sort();

            OnInventoryChanged?.Invoke(list);
        }
    }
}