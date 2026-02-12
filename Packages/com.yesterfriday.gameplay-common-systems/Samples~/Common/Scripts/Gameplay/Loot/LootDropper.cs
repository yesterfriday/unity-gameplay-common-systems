using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay.Loot
{
    public sealed class LootDropper : MonoBehaviour
    {
        [Header("Table")]
        [SerializeField] private LootTableDefinition _lootTable;

        [Header("Spawn")]
        [SerializeField] private LootPickup2D _pickupPrefab;
        [SerializeField] private Transform _dropOrigin;
        [SerializeField] private int _dropCount = 1;
        [SerializeField] private float _scatterRadius = 0.4f;

        public bool TryDrop()
        {
            if (_lootTable == null)
            {
                Debug.LogWarning($"{nameof(LootDropper)}: Missing loot table.", this);
                return false;
            }

            if (_pickupPrefab == null)
            {
                Debug.LogError($"{nameof(LootDropper)}: Missing pickup prefab.", this);
                return false;
            }

            if (_dropOrigin == null)
                _dropOrigin = transform;

            bool anyDropped = false;
            int count = Mathf.Max(1, _dropCount);

            for (int i = 0; i < count; i++)
            {
                if (!_lootTable.TryRoll(out var loot))
                    continue;

                var offset = (Vector2)UnityEngine.Random.insideUnitCircle * _scatterRadius;
                var pos = (Vector2)_dropOrigin.position + offset;

                var pickup = Instantiate(_pickupPrefab, pos, Quaternion.identity);
                pickup.SetLoot(loot);
                anyDropped = true;
            }

            return anyDropped;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_dropCount < 1) _dropCount = 1;
            if (_scatterRadius < 0f) _scatterRadius = 0f;
        }
#endif
    }
}