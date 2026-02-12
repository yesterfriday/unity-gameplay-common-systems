using System;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay
{
    /// <summary>
    /// Fires WaveCleared once when (armed) and enemies go from >0 to 0.
    /// Prevents false positive when a wave starts with 0 alive.
    /// </summary>
    public sealed class WaveEndCondition_EliminateAll : MonoBehaviour
    {
        [SerializeField] private EnemyRegistry _enemyRegistry;
        [SerializeField] private bool _requireAtLeastOneEnemy = true;

        public event Action WaveCleared;

        private bool _isArmed;
        private bool _hasFired;
        private bool _hasSeenAnyEnemy;

        public bool IsArmed => _isArmed;

        private void OnEnable()
        {
            if (_enemyRegistry != null)
                _enemyRegistry.AliveCountChanged += HandleAliveCountChanged;
        }

        private void OnDisable()
        {
            if (_enemyRegistry != null)
                _enemyRegistry.AliveCountChanged -= HandleAliveCountChanged;
        }

        public void Arm()
        {
            _isArmed = true;
            _hasFired = false;
            _hasSeenAnyEnemy = !_requireAtLeastOneEnemy;

            if (_enemyRegistry != null && _enemyRegistry.AliveCount > 0)
                _hasSeenAnyEnemy = true;
        }

        public void Disarm()
        {
            _isArmed = false;
            _hasFired = false;
            _hasSeenAnyEnemy = false;
        }

        private void HandleAliveCountChanged(int aliveCount)
        {
            if (!_isArmed || _hasFired)
                return;

            if (aliveCount > 0)
            {
                _hasSeenAnyEnemy = true;
                return;
            }

            if (!_hasSeenAnyEnemy)
                return;

            _hasFired = true;
            WaveCleared?.Invoke();
        }
    }
}