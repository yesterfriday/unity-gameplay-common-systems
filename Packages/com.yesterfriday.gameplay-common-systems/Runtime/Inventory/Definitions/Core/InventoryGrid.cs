using System;

namespace Yesterfriday.GameplayCommonSystems.Inventory
{
    /// <summary>
    /// Fixed-size grid inventory storage.
    /// Indexing: row-major (left->right, top->bottom)
    /// index = y * width + x
    /// </summary>
    public sealed class InventoryGrid
    {
        private int Width { get;}
        private int Height { get;}

        public int SlotCount => Width * Height; 

        private readonly ItemStack[] _slots;
        
        public InventoryGrid(int width, int height)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }
            if(height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }
            
            Width = width;
            Height = height;
            _slots = new ItemStack[SlotCount];
        }

        public bool IsValidIndex(int index)
        {
            return index >= 0 && index < _slots.Length;
        }

        public int ToIndex(int x, int y)
        {
            if (x < 0 || x >= Width) 
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            if( y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }
            
            return y * Width + x;
        }

        public void ToXY(int index, out int x, out int y)
        {
            if (!IsValidIndex(index))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            
            x = index % Width;
            y = index / Width;
        }
        
        public ItemStack GetSlot(int index)
        {
            if (!IsValidIndex(index))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return _slots[index];
        }

        public void SetSlot(int index, ItemStack stack)
        {
            if (!IsValidIndex(index))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            _slots[index] = stack;
        }
        
        public void ClearSlot(int index)
        {
            if (!IsValidIndex(index)) throw new ArgumentOutOfRangeException(nameof(index));
            _slots[index].Clear();
        }

        public bool IsSlotEmpty(int index)
        {
            if (!IsValidIndex(index)) throw new ArgumentOutOfRangeException(nameof(index));
            return _slots[index].IsEmpty;
        }
    }
}