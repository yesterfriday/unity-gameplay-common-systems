using UnityEngine;
using Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay;
using Yesterfriday.GameplayCommonSystems.MonsterSpawner;

namespace Yesterfriday.GameplayCommonSystems.SamplesGameA
{
    public sealed class GameAFlowCoordinator : MonoBehaviour
    {
        [Header("Refs (Systems)")]
        [SerializeField] private WaveController _waveController;
        [SerializeField] private MonsterSpawner.MonsterSpawner _monsterSpawner;
        [SerializeField] private EnemyRegistry _enemyRegistry;
        [SerializeField] private WaveEndCondition_EliminateAll _waveEndCondition;

        [Header("Debug Spawn")]
        [SerializeField] private MonsterDefinition _debugMonster;
        [SerializeField] private KeyCode _spawnKey = KeyCode.F1;

        private void Awake()
        {
            if (_enemyRegistry != null)
                _enemyRegistry.AliveCountChanged += OnAliveCountChanged;

            if (_waveEndCondition != null)
                _waveEndCondition.WaveCleared += OnWaveCleared;
        }

        private void OnDestroy()
        {
            if (_enemyRegistry != null)
                _enemyRegistry.AliveCountChanged -= OnAliveCountChanged;

            if (_waveEndCondition != null)
                _waveEndCondition.WaveCleared -= OnWaveCleared;
        }

        private void Start()
        {
            Debug.Log("[GameA] Initialized.", this);

            if (_waveEndCondition != null)
            {
                _waveEndCondition.Arm();
                Debug.Log("[GameA] WaveEndCondition armed.", this);
            }

            if (_monsterSpawner == null) Debug.LogWarning("[GameA] Missing MonsterSpawner ref.", this);
            if (_debugMonster == null) Debug.LogWarning("[GameA] Missing Debug MonsterDefinition ref.", this);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(_spawnKey)) return;

            if (_monsterSpawner == null || _debugMonster == null)
            {
                Debug.LogWarning("[GameA] Spawn skipped: missing spawner or debug monster.", this);
                return;
            }

            bool ok = _monsterSpawner.TrySpawn(_debugMonster, out var instance);
            Debug.Log($"[GameA] TrySpawn({_debugMonster.name}) -> {ok}, instance={(instance ? instance.name : "null")}", this);
        }

        private void OnAliveCountChanged(int aliveCount)
        {
            Debug.Log($"[GameA] AliveCountChanged -> {aliveCount}", this);
        }

        private void OnWaveCleared()
        {
            Debug.Log("[GameA] WaveCleared.", this);
        }
    }
}