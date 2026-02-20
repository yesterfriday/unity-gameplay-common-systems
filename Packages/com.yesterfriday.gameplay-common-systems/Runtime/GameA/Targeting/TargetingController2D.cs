using System;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Samples.GameA.Targeting
{
    public sealed class TargetingController2D : MonoBehaviour
    {
        private enum State
        {
            Idle,
            AwaitTarget
        }

        [Header("Refs")]
        [SerializeField] private Camera _camera;

        [Header("Config")]
        [SerializeField] private LayerMask _targetMask = ~0;

        public event Action<Targetable2D> TargetChanged;
        public event Action<Collider2D> HitColliderChanged;
        public event Action<bool> TargetingModeChanged;

        public bool IsTargeting => _state == State.AwaitTarget;
        public Targetable2D CurrentTarget => _currentTarget;

        // ✅ collider 기준: 현재 선택된 히트 콜라이더
        public Collider2D CurrentHitCollider => _currentHitCollider;

        private State _state = State.Idle;
        private Targetable2D _currentTarget;
        private Collider2D _currentHitCollider;

        private void Awake()
        {
            if (_camera == null) _camera = Camera.main;
        }

        public void BeginTargeting()
        {
            if (_state == State.AwaitTarget) return;

            _state = State.AwaitTarget;
            TargetingModeChanged?.Invoke(true);
        }

        /// <summary>
        /// ✅ Esc 용: 모드는 유지하고, 선택만 지움
        /// </summary>
        public void ClearSelection()
        {
            if (_currentTarget == null && _currentHitCollider == null) return;
            SetSelection(null, null);
        }

        /// <summary>
        /// ✅ 우클릭 용: 모드 종료(Idle) + 선택 지움
        /// </summary>
        public void EndTargeting()
        {
            if (_state == State.Idle && _currentTarget == null && _currentHitCollider == null) return;

            _state = State.Idle;
            SetSelection(null, null);
            TargetingModeChanged?.Invoke(false);
        }

        private void Update()
        {
            if (_state != State.AwaitTarget)
                return;

            // ✅ Esc = 클리어(모드 유지)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearSelection();
                return;
            }

            // ✅ 우클릭 = 모드 종료
            if (Input.GetMouseButtonDown(1))
            {
                EndTargeting();
                return;
            }

            // 좌클릭 = 선택/교체
            if (Input.GetMouseButtonDown(0))
            {
                TryPickAt(Input.mousePosition);
            }
        }

        private void TryPickAt(Vector3 screenPos)
        {
            if (_camera == null) return;

            // Ortho라도 안전하게 z 보정(카메라 z=-40이면 40)
            var sp = screenPos;
            sp.z = -_camera.transform.position.z;

            Vector3 world = _camera.ScreenToWorldPoint(sp);
            Vector2 point = new Vector2(world.x, world.y);

            Collider2D hit = Physics2D.OverlapPoint(point, _targetMask);

            if (hit == null)
            {
                // 빈 공간 클릭 = 클리어
                SetSelection(null, null);
                return;
            }

            // ✅ collider 기준 유지
            // targetable은 "연계용"(카드/데미지/하이라이트에서 묶음 단위로 필요)
            Targetable2D targetable =
                hit.attachedRigidbody != null
                    ? hit.attachedRigidbody.GetComponent<Targetable2D>()
                    : hit.GetComponentInParent<Targetable2D>();

            // targetable이 없더라도 collider 선택만 유지할지 정책 선택 가능.
            // v0.1 권장: Targetable이 없으면 선택 무시(프리팹 구성 강제)
            if (targetable == null)
            {
                // 원하면 여기서 SetSelection(null, hit)로 "collider만" 유지도 가능하지만,
                // GameA 흐름에선 Targetable을 전제로 하는 편이 안전함.
                return;
            }

            SetSelection(targetable, hit);
        }

        private void SetSelection(Targetable2D target, Collider2D hitCollider)
        {
            bool targetChanged = _currentTarget != target;
            bool colliderChanged = _currentHitCollider != hitCollider;

            if (!targetChanged && !colliderChanged) return;

            _currentTarget = target;
            _currentHitCollider = hitCollider;

            if (colliderChanged) HitColliderChanged?.Invoke(_currentHitCollider);
            if (targetChanged) TargetChanged?.Invoke(_currentTarget);
        }
    }
}