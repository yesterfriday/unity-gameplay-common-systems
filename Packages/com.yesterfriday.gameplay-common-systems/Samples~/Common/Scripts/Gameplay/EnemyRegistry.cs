using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay
{
    /// <summary>
    /// Tracks currently-alive enemies for wave logic and targeting.
    /// Try* returns true only when registry state actually changes.
    /// </summary>
    public sealed class EnemyRegistry : MonoBehaviour
    {
        public event Action<int> AliveCountChanged;
        public event Action<Transform> EnemyRegistered;
        public event Action<Transform> EnemyUnregistered;

        private readonly HashSet<Transform> _enemies = new();

        public int AliveCount => _enemies.Count;
        public IReadOnlyCollection<Transform> Enemies => _enemies;

        public bool TryRegister(Transform enemy)
        {
            if (enemy == null)
            {
                Debug.LogWarning($"{nameof(EnemyRegistry)}: TryRegister ignored (enemy is null).", this);
                return false;
            }

            if (!_enemies.Add(enemy))
            {
                Debug.LogWarning($"{nameof(EnemyRegistry)}: Duplicate register ignored ({enemy.name}).", enemy);
                return false;
            }

            EnemyRegistered?.Invoke(enemy);
            AliveCountChanged?.Invoke(_enemies.Count);
            return true;
        }

        public bool TryUnregister(Transform enemy)
        {
            if (enemy == null)
            {
                Debug.LogWarning($"{nameof(EnemyRegistry)}: TryUnregister ignored (enemy is null).", this);
                return false;
            }

            if (!_enemies.Remove(enemy))
            {
                Debug.LogWarning($"{nameof(EnemyRegistry)}: Unregister ignored (not found: {enemy.name}).", enemy);
                return false;
            }

            EnemyUnregistered?.Invoke(enemy);
            AliveCountChanged?.Invoke(_enemies.Count);
            return true;
        }

        public bool TryClear()
        {
            if (_enemies.Count <= 0) return false;

            _enemies.Clear();
            AliveCountChanged?.Invoke(0);
            return true;
        }
    }
}