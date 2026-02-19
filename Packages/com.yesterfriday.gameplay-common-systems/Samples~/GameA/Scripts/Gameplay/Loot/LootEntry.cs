using System;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay.Loot
{
    [Serializable]
    public struct LootEntry
    {
        [SerializeField] private string _itemId;
        [SerializeField] private float _weight;
        [SerializeField] private int _minAmount;
        [SerializeField] private int _maxAmount;

        public string ItemId => _itemId;
        public float Weight => _weight;
        public int MinAmount => _minAmount;
        public int MaxAmount => _maxAmount;

#if UNITY_EDITOR
        public void OnValidate()
        {
            _itemId = (_itemId ?? string.Empty).Trim();

            if (_weight < 0f) _weight = 0f;
            if (_minAmount < 1) _minAmount = 1;
            if (_maxAmount < _minAmount) _maxAmount = _minAmount;
        }
#endif
    }
}