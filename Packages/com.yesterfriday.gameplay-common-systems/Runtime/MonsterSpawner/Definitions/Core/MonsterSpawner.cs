using System;
using System.Collections.Generic;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.MonsterSpawner
{
    public sealed class MonsterSpawner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();
        [SerializeField] private SpawnPointSelection _selection = SpawnPointSelection.Random;
        [SerializeField] private int _maxAlive = 10;
        [SerializeField] private float _cooldownSeconds = 0.5f;

        // Runtime state
        private float _nextSpawnTime;
        private int _nextSequentialIndex;

        private readonly List<GameObject> _alive = new List<GameObject>();
        private readonly Dictionary<GameObject, MonsterDefinition> _aliveMap = new Dictionary<GameObject, MonsterDefinition>();

        // Events
        public event Action<MonsterDefinition, GameObject, int> OnSpawned;
        public event Action<MonsterDefinition, GameObject> OnDespawned;
        public event Action<int> OnAliveCountChanged;

        // Queries
        public int AliveCount => _alive.Count;
        public IReadOnlyList<GameObject> AliveInstances => _alive;


        // Public API (v0.1)
        public bool TrySpawn(MonsterDefinition def, out GameObject instance)
        {
            instance = null;

            if (!CanSpawnCommon(def))
            {
                return false;
            }

            if (!TrySelectSpawnPoint(out int index, out Transform point))
            {
                return false;
            }

            return SpawnAt(def, index, point, out instance);
        }

        public bool TrySpawnAt(MonsterDefinition def, int spawnPointIndex, out GameObject instance)
        {
            instance = null;

            if (!CanSpawnCommon(def))
            {
                return false;
            }

            if ((uint)spawnPointIndex >= (uint)_spawnPoints.Count)
            {
                return false;
            }

            var point = _spawnPoints[spawnPointIndex];
            if (point == null)
            {
                return false;
            }

            return SpawnAt(def, spawnPointIndex, point, out instance);
        }

        public bool TryDespawn(GameObject instance)
        {
            if (instance == null)
            {
                return false;
            }

            // 추적 중인지 확인 + def 획득
            if (!_aliveMap.TryGetValue(instance, out MonsterDefinition def))
            {
                return false;
            }

            // 추적 제거 (순서 중요)
            _aliveMap.Remove(instance);
            _alive.Remove(instance);

            OnDespawned?.Invoke(def, instance);
            OnAliveCountChanged?.Invoke(_alive.Count);

            Destroy(instance);
            return true;
        }


        // Internal helpers
        private bool CanSpawnCommon(MonsterDefinition def)
        {
            if (def == null)
            {
                return false;
            }
            if (def.Prefab == null)
            {
                return false;
            }

            if (_spawnPoints == null || _spawnPoints.Count == 0)
            {
                return false;
            }

            // MaxAlive: >= 이면 실패
            if (_alive.Count >= _maxAlive)
            {
                return false;
            }

            // Cooldown
            if (Time.time < _nextSpawnTime)
            {
                return false;
            }

            return true;
        }

        private bool TrySelectSpawnPoint(out int index, out Transform point)
        {
            index = -1;
            point = null;

            int count = _spawnPoints.Count;

            switch (_selection)
            {
                case SpawnPointSelection.Random:
                    index = UnityEngine.Random.Range(0, count);
                    break;

                case SpawnPointSelection.Sequential:
                    index = _nextSequentialIndex % count;
                    _nextSequentialIndex = (index + 1) % count;
                    break;

                default:
                    return false;
            }

            point = _spawnPoints[index];
            if (point == null)
            {
                return false;
            }

            return true;
        }

        private bool SpawnAt(MonsterDefinition def, int spawnPointIndex, Transform point, out GameObject instance)
        {
            instance = Instantiate(def.Prefab, point.position, point.rotation);

            _alive.Add(instance);
            _aliveMap[instance] = def;

            _nextSpawnTime = Time.time + _cooldownSeconds;

            OnSpawned?.Invoke(def, instance, spawnPointIndex);
            OnAliveCountChanged?.Invoke(_alive.Count);

            return true;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_maxAlive < 0)
            {
                _maxAlive = 0;
            }
            if (_cooldownSeconds < 0f)
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
