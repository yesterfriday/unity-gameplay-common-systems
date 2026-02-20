using UnityEngine;
using Yesterfriday.GameplayCommonSystems.SamplesCommon.Gameplay;

namespace Yesterfriday.GameplayCommonSystems.SamplesGameA
{
    public sealed class EnemyAutoRegister2D : MonoBehaviour
    {
        [Header("Optional override")]
        [SerializeField] private EnemyRegistry _overrideRegistry;

        [Header("Registry Key (use root instance transform)")]
        [SerializeField] private Transform _registryKeyOverride;

        private EnemyRegistry _registry;
        private bool _unregisterAttempted;
        private bool _warnedMissingRegistry;

        private void Awake()
        {
            _registry = _overrideRegistry != null
                ? _overrideRegistry
                : FindFirstObjectByType<EnemyRegistry>();

            // ✅ 기본: override가 없으면 루트로 설정 (자식에 붙어 있어도 루트 키로 Unregister)
            if (_registryKeyOverride == null)
                _registryKeyOverride = transform.root;
        }

        private void OnDisable()
        {
            TryUnregisterOnce();
        }

        private void OnDestroy()
        {
            TryUnregisterOnce();
        }

        private void TryUnregisterOnce()
        {
            if (_unregisterAttempted) return;
            _unregisterAttempted = true;

            if (_registry == null)
            {
                if (!_warnedMissingRegistry)
                {
                    _warnedMissingRegistry = true;
                    Debug.LogWarning("[GameA] EnemyAutoRegister2D: EnemyRegistry not found in scene (unregister skipped).", this);
                }
                return;
            }

            Transform key = _registryKeyOverride != null ? _registryKeyOverride : transform.root;
            _registry.TryUnregister(key);
        }
    }
}