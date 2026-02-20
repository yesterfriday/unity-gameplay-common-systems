using UnityEngine;
using Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay;
using Yesterfriday.GameplayCommonSystems.MonsterSpawner;
using Yesterfriday.GameplayCommonSystems.Samples.GameA.Targeting;

namespace Yesterfriday.GameplayCommonSystems.SamplesGameA
{
    public sealed class GameAFlowCoordinator : MonoBehaviour
    {
        [Header("Refs (Systems)")]
        [SerializeField] private WaveController _waveController;
        [SerializeField] private MonsterSpawner.MonsterSpawner _monsterSpawner;
        [SerializeField] private EnemyRegistry _enemyRegistry;
        [SerializeField] private WaveEndCondition_EliminateAll _waveEndCondition;
        [SerializeField] private TargetingController2D _targeting;
        
        [Header("Debug Spawn")]
        [SerializeField] private MonsterDefinition _debugMonster;
        [SerializeField] private KeyCode _spawnKey = KeyCode.F1;
        [SerializeField] private KeyCode _startWaveKey = KeyCode.F2;
        [SerializeField] private bool _autoStartWaveOnPlay = true;
        [SerializeField] private bool _autoStartNextWaveOnEnd = true;
        
        private void Awake()
        {
            if (_enemyRegistry != null)
            {
                _enemyRegistry.AliveCountChanged += OnAliveCountChanged;
            }

            if (_waveEndCondition != null)
            {
                _waveEndCondition.WaveCleared += OnWaveCleared;
            }
        }

        private void OnDestroy()
        {
            if (_enemyRegistry != null)
            {
                _enemyRegistry.AliveCountChanged -= OnAliveCountChanged;
            }

            if (_waveEndCondition != null)
            {
                _waveEndCondition.WaveCleared -= OnWaveCleared;
            }
            
            if (_targeting == null)
            {
                return;
            }
            _targeting.TargetChanged -= OnTargetChanged;
            _targeting.TargetingModeChanged -= enabled => Debug.Log($"Targeting: {enabled}");
        }

        private void Start()
        {
            Debug.Log("[GameA] Initialized.", this);

            if (_waveEndCondition != null)
            {
                _waveEndCondition.Arm();
                Debug.Log("[GameA] WaveEndCondition armed.", this);
            }

            if (_monsterSpawner == null)
            {
                Debug.LogWarning("[GameA] Missing MonsterSpawner ref.", this);
            }
            
            if (_debugMonster == null)
            {
                Debug.LogWarning("[GameA] Missing Debug MonsterDefinition ref.", this);
            }
            
            if (_autoStartWaveOnPlay && _waveController != null)
            {
                bool started = _waveController.TryStartNextWave();
                Debug.Log($"[GameA] TryStartNextWave (auto) -> {started}", this);
            }
            
            _targeting.BeginTargeting();
            _targeting.TargetChanged += OnTargetChanged;
            _targeting.TargetingModeChanged += enabled => Debug.Log($"Targeting: {enabled}");
        }

        private void Update()
        {
            if (Input.GetKeyDown(_startWaveKey))
            {
                if (_waveController == null)
                {
                    Debug.LogWarning("[GameA] StartWave skipped: missing WaveController ref.", this);
                }
                else
                {
                    bool started = _waveController.TryStartNextWave();
                    Debug.Log($"[GameA] TryStartNextWave (key={_startWaveKey}) -> {started}", this);

                    // 웨이브마다 EndCondition 재무장(안전)
                    if (started && _waveEndCondition != null)
                    {
                        _waveEndCondition.Arm();
                        Debug.Log("[GameA] WaveEndCondition armed (on wave start).", this);
                    }
                }
            }
            
            if (!Input.GetKeyDown(_spawnKey))
            {
                return;
            }

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
            
            if (_waveController != null && _waveController.IsRunning)
            {
                bool ended = _waveController.TryEndWave();
                Debug.Log($"[GameA] TryEndWave -> {ended}", this);
            }

            if (_waveEndCondition != null)
            {
                _waveEndCondition.Disarm();
                Debug.Log("[GameA] WaveEndCondition disarmed.", this);
            }

            if (_autoStartNextWaveOnEnd && _waveController != null)
            {
                bool started = _waveController.TryStartNextWave();
                Debug.Log($"[GameA] TryStartNextWave (auto-next) -> {started}", this);

                if (started && _waveEndCondition != null)
                {
                    _waveEndCondition.Arm();
                    Debug.Log("[GameA] WaveEndCondition armed (auto-next).", this);
                }
            }
        }
        
        private void OnTargetChanged(Targetable2D t)
        {
            Debug.Log(t != null ? $"Target = {t.name}" : "Target = None");
        }
    }
}