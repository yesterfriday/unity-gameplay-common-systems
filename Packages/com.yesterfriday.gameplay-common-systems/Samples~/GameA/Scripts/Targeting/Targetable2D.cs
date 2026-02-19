using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Samples.GameA.Targeting
{
    /// <summary>
    /// 클릭 타겟으로 선택될 수 있는 대상(적 등)에 붙이는 컴포넌트.
    /// collider 기준 연계를 위해 PrimaryCollider를 제공한다.
    /// </summary>
    public sealed class Targetable2D : MonoBehaviour
    {
        [Header("Optional")]
        [SerializeField] private Collider2D _primaryCollider;

        public Collider2D PrimaryCollider => _primaryCollider != null
            ? _primaryCollider
            : (_primaryCollider = GetComponentInChildren<Collider2D>());

        private void OnValidate()
        {
            if (_primaryCollider == null)
            {
                _primaryCollider = GetComponentInChildren<Collider2D>();
            }

            if (_primaryCollider == null)
            {
                Debug.LogWarning($"{nameof(Targetable2D)}: No Collider2D found in children.", this);
            }
        }
    }
}