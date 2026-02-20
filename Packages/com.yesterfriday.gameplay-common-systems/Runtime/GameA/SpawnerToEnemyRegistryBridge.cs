using UnityEngine;
using Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay;
using Yesterfriday.GameplayCommonSystems.MonsterSpawner;

namespace Yesterfriday.GameplayCommonSystems.SamplesGameA
{
    public sealed class SpawnerToEnemyRegistryBridge : MonoBehaviour
    {
        [SerializeField] private MonsterSpawner.MonsterSpawner _spawner;
        [SerializeField] private EnemyRegistry _registry;

        private void Awake()
        {
            if (_spawner == null) _spawner = FindFirstObjectByType<MonsterSpawner.MonsterSpawner>();
            if (_registry == null) _registry = FindFirstObjectByType<EnemyRegistry>();

            if (_spawner != null)
            {
                _spawner.OnSpawned += OnSpawned;
                _spawner.OnDespawned += OnDespawned;
            }
        }

        private void OnDestroy()
        {
            if (_spawner != null)
            {
                _spawner.OnSpawned -= OnSpawned;
                _spawner.OnDespawned -= OnDespawned;
            }
        }

        private void OnSpawned(MonsterDefinition def, GameObject instance, int spawnPointIndex)
        {
            if (_registry == null || instance == null) return;
            _registry.TryRegister(instance.transform);
        }

        private void OnDespawned(MonsterDefinition def, GameObject instance)
        {
            if (_registry == null || instance == null) return;
            _registry.TryUnregister(instance.transform);
        }
    }
}