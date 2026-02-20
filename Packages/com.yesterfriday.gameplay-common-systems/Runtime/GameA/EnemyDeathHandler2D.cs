using UnityEngine;
using Yesterfriday.GameplayCommonSystems.Samples.Common.Gameplay;
using Yesterfriday.GameplayCommonSystems.Samples.GameA.Targeting;
using Yesterfriday.GameplayCommonSystems.SamplesGameA;

namespace Yesterfriday.GameplayCommonSystems.GameA.Gameplay
{
    /// <summary>
    /// Enemy의 자식 오브젝트에 붙여도 동작하도록:
    /// - Health: parent/children에서 자동 탐색하여 Died 구독
    /// - RootToDisable: parent chain에서 "등록/타겟 루트"를 우선 탐색 후 disable
    /// P0 목표: 죽으면 루트를 비활성화하여 WaveCleared 조건을 만족시킨다.
    /// </summary>
    public sealed class EnemyDeathHandler2D : MonoBehaviour
    {
        [Header("Refs (Optional)")]
        [SerializeField] private Health _health;

        [Tooltip("비워두면 자동으로 Enemy 루트를 찾아 비활성화합니다.")]
        [SerializeField] private GameObject _rootToDisable;

        [Header("Config")]
        [Tooltip("죽었을 때 Destroy가 아니라 SetActive(false) 처리")]
        [SerializeField] private bool _disableOnDeath = true;

        private bool _handled;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnEnable()
        {
            _handled = false;
            if (_health != null)
                _health.Died += HandleDied;
        }

        private void OnDisable()
        {
            if (_health != null)
                _health.Died -= HandleDied;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // 편의: 인스펙터에서 비워둬도 참조가 잡히도록.
            // (OnValidate는 런타임이 아니라 편집기에서만)
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
                ResolveReferences();
        }
#endif

        private void ResolveReferences()
        {
            if (_health == null)
            {
                // 자식에 붙어 있어도 동작해야 하므로 parent/children에서 탐색
                _health = GetComponentInParent<Health>(true);
                if (_health == null) _health = GetComponentInChildren<Health>(true);
            }

            if (_rootToDisable == null)
            {
                // 1) EnemyAutoRegister2D가 붙은 오브젝트를 루트로 간주(가장 안정적)
                var autoRegister = GetComponentInParent<EnemyAutoRegister2D>(true);
                if (autoRegister != null)
                {
                    _rootToDisable = autoRegister.gameObject;
                    return;
                }

                // 2) Targetable2D가 있다면 그 오브젝트를 루트로 간주(타겟/피격 루트)
                var targetable = GetComponentInParent<Targetable2D>(true);
                if (targetable != null)
                {
                    _rootToDisable = targetable.gameObject;
                    return;
                }

                // 3) 최후의 fallback: 프리팹 최상위
                _rootToDisable = transform.root.gameObject;
            }
        }

        private void HandleDied()
        {
            if (_handled) return;
            _handled = true;

            if (_rootToDisable == null)
            {
                Debug.LogWarning("[EnemyDeathHandler2D] RootToDisable is null.", this);
                return;
            }

            if (_disableOnDeath)
            {
                if (_rootToDisable.activeSelf)
                    _rootToDisable.SetActive(false);
            }
            else
            {
                Destroy(_rootToDisable);
            }
        }
    }
}