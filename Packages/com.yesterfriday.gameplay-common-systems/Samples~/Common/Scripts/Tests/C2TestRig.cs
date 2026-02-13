using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay;
using Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay.Loot;

namespace Yesterfriday.GameplayCommonSystems.SamplesCommon.Tests
{
    public sealed class C2TestRig : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private EnemyRegistry _enemyRegistry;
        [SerializeField] private WaveEndCondition_EliminateAll _waveEndCondition;
        [SerializeField] private LootDropper _lootDropper;
        [SerializeField] private SimplePlayerMover2D _playerMover;

        [Header("Dummy Enemies")]
        [SerializeField] private int _dummyEnemyLayer = 0; // Default
        [SerializeField] private Vector2 _dummySpawnCenter = new(3f, 0f);
        [SerializeField] private float _dummySpawnRadius = 1.5f;

        private readonly List<Transform> _spawnedEnemies = new();
        private readonly HashSet<LootPickup2D> _subscribedPickups = new();

        private void Awake()
        {
            if (_enemyRegistry != null)
                _enemyRegistry.AliveCountChanged += OnAliveCountChanged;

            if (_waveEndCondition != null)
                _waveEndCondition.WaveCleared += OnWaveCleared;
            
            if (_lootDropper != null)
                    _lootDropper.PickupSpawned += OnPickupSpawned;
        }

        private void OnDestroy()
        {
            if (_enemyRegistry != null)
                _enemyRegistry.AliveCountChanged -= OnAliveCountChanged;

            if (_waveEndCondition != null)
                _waveEndCondition.WaveCleared -= OnWaveCleared;
            
            if (_lootDropper != null)
                    _lootDropper.PickupSpawned -= OnPickupSpawned;
        }

        private void Update()
        {
            // F1: Spawn+Register enemy
            if (Input.GetKeyDown(KeyCode.F1))
                SpawnAndRegisterEnemy();

            // F2: Unregister+Destroy last enemy
            if (Input.GetKeyDown(KeyCode.F2))
                UnregisterAndDestroyLastEnemy();

            // F3: Arm wave end condition
            if (Input.GetKeyDown(KeyCode.F3))
                Arm();

            // F4: Disarm wave end condition
            if (Input.GetKeyDown(KeyCode.F4))
                Disarm();

            // F5: Drop loot
            if (Input.GetKeyDown(KeyCode.F5))
                DropLoot();

            // F6: Duplicate/invalid ops test
            if (Input.GetKeyDown(KeyCode.F6))
                RunDuplicateAndInvalidOps();
        }

        private void OnAliveCountChanged(int aliveCount)
        {
            Debug.Log($"[C2TestRig] AliveCountChanged -> {aliveCount}", this);
        }

        private void OnWaveCleared()
        {
            Debug.Log("[C2TestRig] WaveCleared fired (should be ONCE when >0 -> 0 while armed).", this);
        }

        private void Arm()
        {
            if (_waveEndCondition == null)
            {
                Debug.LogError("[C2TestRig] Missing WaveEndCondition ref.", this);
                return;
            }

            _waveEndCondition.Arm();
            Debug.Log("[C2TestRig] Arm()", this);
        }

        private void Disarm()
        {
            if (_waveEndCondition == null)
            {
                Debug.LogError("[C2TestRig] Missing WaveEndCondition ref.", this);
                return;
            }

            _waveEndCondition.Disarm();
            Debug.Log("[C2TestRig] Disarm()", this);
        }

        private void SpawnAndRegisterEnemy()
        {
            if (_enemyRegistry == null)
            {
                Debug.LogError("[C2TestRig] Missing EnemyRegistry ref.", this);
                return;
            }

            Vector2 pos = _dummySpawnCenter + Random.insideUnitCircle * _dummySpawnRadius;

            var go = new GameObject($"DummyEnemy_{_spawnedEnemies.Count:00}");
            go.layer = _dummyEnemyLayer;
            go.transform.position = pos;

            // Visual aid (optional): add SpriteRenderer if you want
            // go.AddComponent<SpriteRenderer>();

            _spawnedEnemies.Add(go.transform);

            bool changed = _enemyRegistry.TryRegister(go.transform);
            Debug.Log($"[C2TestRig] SpawnAndRegisterEnemy -> changed={changed}, enemy={go.name}", go);
        }

        private void UnregisterAndDestroyLastEnemy()
        {
            if (_enemyRegistry == null)
            {
                Debug.LogError("[C2TestRig] Missing EnemyRegistry ref.", this);
                return;
            }

            if (_spawnedEnemies.Count <= 0)
            {
                Debug.LogWarning("[C2TestRig] No spawned enemies to remove.", this);
                return;
            }

            int last = _spawnedEnemies.Count - 1;
            Transform t = _spawnedEnemies[last];
            _spawnedEnemies.RemoveAt(last);

            bool changed = _enemyRegistry.TryUnregister(t);
            Debug.Log($"[C2TestRig] UnregisterAndDestroyLastEnemy -> changed={changed}, enemy={t.name}", t);

            if (t != null)
                Destroy(t.gameObject);
        }

        private void RunDuplicateAndInvalidOps()
        {
            if (_enemyRegistry == null)
            {
                Debug.LogError("[C2TestRig] Missing EnemyRegistry ref.", this);
                return;
            }

            if (_spawnedEnemies.Count <= 0)
            {
                Debug.LogWarning("[C2TestRig] Spawn at least 1 enemy first (F1).", this);
                return;
            }

            Transform t = _spawnedEnemies[0];

            bool dup = _enemyRegistry.TryRegister(t);
            Debug.Log($"[C2TestRig] Duplicate Register -> changed={dup} (expected false)", t);

            var phantom = new GameObject("PhantomEnemy").transform;
            bool invalid = _enemyRegistry.TryUnregister(phantom);
            Debug.Log($"[C2TestRig] Unregister not-registered -> changed={invalid} (expected false)", phantom);
            Destroy(phantom.gameObject);
        }

        private void DropLoot()
        {
            if (_lootDropper == null)
            {
                Debug.LogError("[C2TestRig] Missing LootDropper ref.", this);
                return;
            }

            bool dropped = _lootDropper.TryDrop();
            Debug.Log($"[C2TestRig] LootDropper.TryDrop -> {dropped}", this);
        }

        private void OnPickupSpawned(LootPickup2D pickup)
        {
            if (pickup == null) return;
        
            if (_subscribedPickups.Contains(pickup)) return;
            _subscribedPickups.Add(pickup);
        
            pickup.LootPicked += OnLootPicked;
        
            Debug.Log($"[C2TestRig] Hooked LootPickup2D -> {pickup.name}", pickup); // ✅ 구독 확인 로그
        }

        private void OnLootPicked(LootStack loot)
        {
            Debug.Log($"[C2TestRig] LootPicked -> {loot}", this);
        }
    }
}