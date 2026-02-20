using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay.Loot
{
    [CreateAssetMenu(menuName = "Common Systems/Samples/Loot Table Definition", fileName = "LootTableDefinition")]
    public sealed class LootTableDefinition : ScriptableObject
    {
        [SerializeField] private List<LootEntry> _entries = new();

        public IReadOnlyList<LootEntry> Entries => _entries;

        /// <summary>
        /// Rolls one entry by weight. Returns false if table is empty or total weight is 0.
        /// </summary>
        public bool TryRoll(out LootStack loot)
        {
            loot = default;

            if (_entries == null || _entries.Count <= 0)
                return false;

            float totalWeight = 0f;
            for (int i = 0; i < _entries.Count; i++)
                totalWeight += Mathf.Max(0f, _entries[i].Weight);

            if (totalWeight <= 0f)
                return false;

            float r = UnityEngine.Random.value * totalWeight;
            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                float w = Mathf.Max(0f, e.Weight);
                if (w <= 0f) continue;

                if (r <= w)
                {
                    int amount = UnityEngine.Random.Range(e.MinAmount, e.MaxAmount + 1);
                    loot = new LootStack(e.ItemId, amount);
                    return !string.IsNullOrWhiteSpace(loot.ItemId) && loot.Amount > 0;
                }

                r -= w;
            }

            var last = _entries[_entries.Count - 1];
            int fallbackAmount = UnityEngine.Random.Range(last.MinAmount, last.MaxAmount + 1);
            loot = new LootStack(last.ItemId, fallbackAmount);
            return !string.IsNullOrWhiteSpace(loot.ItemId) && loot.Amount > 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_entries == null) _entries = new List<LootEntry>();

            for (int i = 0; i < _entries.Count; i++)
            {
                var e = _entries[i];
                e.OnValidate();
                _entries[i] = e;
            }
        }
#endif
    }
}