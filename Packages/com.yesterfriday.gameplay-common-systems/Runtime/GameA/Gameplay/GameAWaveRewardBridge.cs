using UnityEngine;
using Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay;
using Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay.Loot;
using Yesterfriday.GameplayCommonSystems.GameA.Inventory;

namespace Yesterfriday.GameplayCommonSystems.GameA.Rewards
{
    /// <summary>
    /// P0: WaveCleared -> TryDrop -> subscribe pickup -> add to inventory counter.
    /// Passive bridge: does NOT Arm/Disarm end condition (FlowCoordinator owns that).
    /// </summary>
    public sealed class GameAWaveRewardBridge : MonoBehaviour
    {
        [Header("Wave Source")]
        [SerializeField] private WaveEndCondition_EliminateAll _endCondition;
        [SerializeField] private WaveController _waveController; // optional

        [Header("Loot")]
        [SerializeField] private LootDropper _lootDropper;

        [Header("Inventory (proof)")]
        [SerializeField] private GameAInventoryCounter _inventory;

        private int _currentWaveIndex = -1;
        private int _lastRewardedWaveIndex = -1;
        
        private bool _rewardedThisWave;

        private void Awake()
        {
            if (_waveController == null)
            {
                Debug.LogError("[GameA][RewardBridge] WaveController reference is missing. Assign it in the scene.",
                    this);
            }
        }
        
        private void OnEnable()
        {
            if (_endCondition != null)
                _endCondition.WaveCleared += HandleWaveCleared;

            if (_lootDropper != null)
                _lootDropper.PickupSpawned += HandlePickupSpawned;

            if (_waveController != null)
            {
                _waveController.OnWaveStarted += HandleWaveStarted;
                _waveController.OnWaveEnded += HandleWaveEnded;
            }
        }

        private void OnDisable()
        {
            if (_waveController != null)
            {
                _waveController.OnWaveStarted -= HandleWaveStarted;
                _waveController.OnWaveEnded -= HandleWaveEnded;
            }

            if (_lootDropper != null)
                _lootDropper.PickupSpawned -= HandlePickupSpawned;

            if (_endCondition != null)
                _endCondition.WaveCleared -= HandleWaveCleared;
        }

        private void HandleWaveStarted(int waveIndex)
        {
            _currentWaveIndex = waveIndex;
        }
        private void HandleWaveEnded(int waveIndex) => _rewardedThisWave = false;

        private void HandleWaveCleared()
        {
            // WaveController가 없으면 "웨이브 단위 1회" 보상 보장이 어려워서 P0에서는 막는 게 안전
            if (_waveController == null)
            {
                Debug.LogWarning("[GameA][RewardBridge] WaveController is required to gate rewards per wave.", this);
                return;
            }

            if (_lastRewardedWaveIndex == _currentWaveIndex)
            {
                return;
            }

            if (_lootDropper == null)
            {
                Debug.LogWarning("[GameA][RewardBridge] Missing LootDropper.", this);
                return;
            }

            bool dropped = _lootDropper.TryDrop();
            Debug.Log($"[GameA][RewardBridge] WaveCleared(wave={_currentWaveIndex}) -> TryDrop={dropped}", this);

            if (dropped)
            {
                _lastRewardedWaveIndex = _currentWaveIndex;
            }
            
        }

        private void HandlePickupSpawned(LootPickup2D pickup)
        {
            if (pickup == null) return;

            // Pickup destroys itself after picked, so no need to unsubscribe.
            pickup.LootPicked += HandleLootPicked;
        }

        private void HandleLootPicked(LootStack loot)
        {
            if (_inventory == null)
            {
                Debug.LogWarning("[GameA][RewardBridge] Missing InventoryCounter.", this);
                return;
            }

            _inventory.TryAdd(loot);
        }
    }
}