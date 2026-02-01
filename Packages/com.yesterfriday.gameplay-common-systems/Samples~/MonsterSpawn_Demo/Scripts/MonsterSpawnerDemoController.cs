using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Yesterfriday.GameplayCommonSystems.MonsterSpawner.Demo
{
    public sealed class MonsterSpawnerDemoController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MonsterSpawner _spawner;
        [SerializeField] private MonsterDefinition _monster;

        [Header("UI (TMP)")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Button _spawnButton;
        [SerializeField] private Button _despawnButton;
        [SerializeField] private Button _spawn10Button;

        private readonly List<GameObject> _spawned = new List<GameObject>();
        
        [SerializeField] private bool _randomizeColorOnSpawn = true;
        [SerializeField] private bool _useGradientColor = true; // false면 랜덤
        private int _spawnSerial = 0;

        private void Reset()
        {
            // 씬에서 자동 할당 도움(선택)
            _spawner = FindObjectOfType<MonsterSpawner>();
        }

        private void OnEnable()
        {
            if (_spawnButton != null) _spawnButton.onClick.AddListener(OnClickSpawn);
            if (_despawnButton != null) _despawnButton.onClick.AddListener(OnClickDespawnLast);
            if (_spawn10Button != null) _spawn10Button.onClick.AddListener(OnClickSpawn10);

            if (_spawner != null)
            {
                _spawner.OnSpawned += HandleSpawned;
                _spawner.OnDespawned += HandleDespawned;
                _spawner.OnAliveCountChanged += HandleAliveCountChanged;
            }

            RefreshUI("Ready");
        }

        private void OnDisable()
        {
            if (_spawnButton != null) _spawnButton.onClick.RemoveListener(OnClickSpawn);
            if (_despawnButton != null) _despawnButton.onClick.RemoveListener(OnClickDespawnLast);
            if (_spawn10Button != null) _spawn10Button.onClick.RemoveListener(OnClickSpawn10);

            if (_spawner != null)
            {
                _spawner.OnSpawned -= HandleSpawned;
                _spawner.OnDespawned -= HandleDespawned;
                _spawner.OnAliveCountChanged -= HandleAliveCountChanged;
            }
        }

        private void OnClickSpawn()
        {
            if (!ValidateRefs())
                return;

            if (_spawner.TrySpawn(_monster, out var instance))
            {
                _spawned.Add(instance);
                ApplyVisual(instance, spawnPointIndex: -1);
                RefreshUI("Spawn success");
            }
            else
            {
                RefreshUI("Spawn failed (cooldown / maxAlive / invalid setup)");
            }
        }

        private void OnClickDespawnLast()
        {
            if (!ValidateRefs())
                return;

            if (_spawned.Count == 0)
            {
                RefreshUI("Despawn failed (no tracked instance)");
                return;
            }

            var last = _spawned[_spawned.Count - 1];
            _spawned.RemoveAt(_spawned.Count - 1);

            if (_spawner.TryDespawn(last))
            {
                RefreshUI("Despawn success");
            }
            else
            {
                RefreshUI("Despawn failed (not tracked / already destroyed)");
            }
        }

        public void OnClickSpawn10()
        {
            if (!ValidateRefs()) return;
            StartCoroutine(SpawnMany(10));
        }

        private IEnumerator SpawnMany(int count)
        {
            int success = 0;

            for (int i = 0; i < count; i++)
            {
                if (_spawner.TrySpawn(_monster, out var instance))
                {
                    _spawned.Add(instance);
                    ApplyVisual(instance, spawnPointIndex: -1);
                    success++;
                }

                // 쿨다운만큼 기다렸다가 다음 시도
                yield return new WaitForSeconds(2f); // <= spawner의 cooldownSeconds와 동일하게
            }

            RefreshUI($"Spawn x{count} done (success: {success})");
        }
        
        private void ApplyVisual(GameObject instance, int spawnPointIndex)
        {
            if (instance == null) return;

            // 1) 색 변경
            if (_randomizeColorOnSpawn)
            {
                var renderer = instance.GetComponentInChildren<Renderer>();
                if (renderer != null)
                {
                    // sharedMaterial을 직접 바꾸면 프리팹/다른 인스턴스까지 같이 바뀔 수 있음
                    // material을 사용하면 인스턴스 전용 머티리얼이 생성됨(데모용 OK)
                    var mat = renderer.material;

                    Color c;
                    if (_useGradientColor)
                    {
                        // 스폰 순번으로 색을 일정하게 변화(발표에서 보기 좋음)
                        float t = (_spawnSerial % 16) / 16f; // 0~1
                        c = Color.HSVToRGB(t, 0.8f, 0.95f);
                    }
                    else
                    {
                        c = UnityEngine.Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.8f, 1f);
                    }

                    if (mat.HasProperty("_Color"))
                        mat.color = c;
                }

                _spawnSerial++;
            }
        }

        
        private void HandleSpawned(MonsterDefinition def, GameObject instance, int spawnPointIndex)
        {
            ApplyVisual(instance, spawnPointIndex);
            
            // 발표용 로그
            Debug.Log($"[Demo] Spawned: {def.name} at P{spawnPointIndex}, Alive={_spawner.AliveCount}");
        }

        private void HandleDespawned(MonsterDefinition def, GameObject instance)
        {
            Debug.Log($"[Demo] Despawned: {def.name}, Alive={_spawner.AliveCount}");
        }

        private void HandleAliveCountChanged(int count)
        {
            RefreshUI(null); // 메시지는 유지하고 카운트만 갱신
        }

        private bool ValidateRefs()
        {
            if (_spawner == null)
            {
                RefreshUI("ERROR: Spawner not assigned");
                return false;
            }

            if (_monster == null)
            {
                RefreshUI("ERROR: MonsterDefinition not assigned");
                return false;
            }

            return true;
        }

        private void RefreshUI(string message)
        {
            if (_titleText != null)
                _titleText.text = "MonsterSpawner Demo";

            int alive = (_spawner != null) ? _spawner.AliveCount : 0;

            if (_statusText != null)
            {
                string msg = string.IsNullOrEmpty(message) ? "" : $"Result: {message}\n";
                _statusText.text =
                    msg +
                    $"AliveCount: {alive}\n" +
                    $"Tracked(Local): {_spawned.Count}";
            }

            if (_despawnButton != null)
                _despawnButton.interactable = _spawned.Count > 0;
        }
    }
}
