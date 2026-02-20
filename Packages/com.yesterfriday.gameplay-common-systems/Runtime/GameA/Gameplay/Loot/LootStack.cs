using System;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay.Loot
{
    [Serializable]
    public struct LootStack
    {
        [SerializeField] private string _itemId;
        [SerializeField] private int _amount;

        public string ItemId => _itemId;
        public int Amount => _amount;

        public LootStack(string itemId, int amount)
        {
            _itemId = itemId;
            _amount = Mathf.Max(0, amount);
        }

        public override string ToString() => $"{_itemId} x{_amount}";
    }
}