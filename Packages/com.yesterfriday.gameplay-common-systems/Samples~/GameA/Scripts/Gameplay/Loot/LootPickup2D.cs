using System;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay.Loot
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class LootPickup2D : MonoBehaviour
    {
        [SerializeField] private LootStack _loot;
        [SerializeField] private LayerMask _playerLayerMask;

        public event Action<LootStack> LootPicked;

        private bool _picked;

        public LootStack Loot => _loot;

        public void SetLoot(LootStack loot) => _loot = loot;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_picked)
                return;

            if (((1 << other.gameObject.layer) & _playerLayerMask.value) == 0)
                return;

            _picked = true;
            LootPicked?.Invoke(_loot);
            Destroy(gameObject);
        }
    }
}