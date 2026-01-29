using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.MonsterSpawner
{
    public sealed class MonsterSpawner : MonoBehaviour
    {
        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
        [SerializeField] private SpawnPointSelection _selection = SpawnPointSelection.Random;
        [SerializeField] private int _maxAlive = 10;
        [SerializeField] private float _cooldownSeconds = 0.5f;

        private float _nextSpawnTime;
        private int _nextSequentialIndex;
        private readonly List<GameObject> _alive = new List<GameObject>();

        private readonly Dictionary<GameObject, MonsterDefinition> _aliveMap = new Dictionary<GameObject, MonsterDefinition>();

        public event Action<MonsterDefinition, GameObject, int> OnSpawned;
        public event Action<MonsterDefinition, GameObject> OnDespawned;
        public event Action<int> OnAliveCountChanged;

        public int AliveCount => _alive.Count;
        public IReadOnlyList<GameObject> AliveInstances => _alive;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_maxAlive < 0)
            {
                _maxAlive = 0;
            }

            if (_cooldownSeconds < 0)
            {
                _cooldownSeconds = 0f;
            }

            if (_spawnPoints == null || _spawnPoints.Count == 0)
            {
                return;
            }
            
            for (int i = 0; i < _spawnPoints.Count; i++)
            {
                if (_spawnPoints[i] == null)
                {
                    Debug.LogWarning($"[MonsterSpawner] SpawnPoint is null at index {i}: {name}", this);
                }
            }
        }
#endif

    }
}