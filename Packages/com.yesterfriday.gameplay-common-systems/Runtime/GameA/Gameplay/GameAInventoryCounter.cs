using System;
using UnityEngine;
using Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay.Loot;

namespace Yesterfriday.GameplayCommonSystems.GameA.Inventory
{
    /// <summary>P0 proof-only: counts loot pick events. Replace later with real inventory.</summary>
    public sealed class GameAInventoryCounter : MonoBehaviour
    {
        [SerializeField] private int _count;
        
        public event Action<int> CountChanged;

        public int Count => _count;


        public bool TryAdd(LootStack loot)
        {
            int prev = _count;
            _count = prev + 1;

            if (_count == prev)
            {
                return false;
            }

            CountChanged?.Invoke(_count);
            Debug.Log($"[GameA][Inventory] Loot picked. Count={_count}", this);
            return true;
        }
    }
}