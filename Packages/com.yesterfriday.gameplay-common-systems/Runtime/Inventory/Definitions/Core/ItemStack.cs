using System;
namespace Yesterfriday.GameplayCommonSystems.Inventory
{
    /// <summary>
    /// Terminology: an "entry" refers to an ItemStack stored in a slot.
    /// </summary>
    [Serializable]
    public struct ItemStack
    {
        public ItemDefinition Item { get; private set; }
        public int Amount { get; private set; }

        public bool IsEmpty => Item == null || Amount <= 0;

        public ItemStack(ItemDefinition item, int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be >= 0");
            }
            
            if (item == null || amount == 0)
            {
                Item = null;
                Amount = 0;
                return;
            }
            
            Item = item;
            Amount = amount;
        }

        public void Clear()
        {
            Item = null;
            Amount = 0;
        }

        public void Set(ItemDefinition item, int amount)
        {

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be >= 0.");
            }
            
            if (item == null || amount <= 0)
            {
                Clear();
                return;
            }

            Item = item;
            Amount = amount;
            
        }

        public void Add(int delta)
        {
            if (delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
            if (IsEmpty) throw new InvalidOperationException("Cannot add to an empty stack. Set Item first.");

            Amount += delta;
        }

        public void Remove(int delta)
        {
            if (delta < 0) throw new ArgumentOutOfRangeException(nameof(delta));
            if (IsEmpty) return;

            Amount -= delta;
            if (Amount <= 0) Clear();
        }
    }
}