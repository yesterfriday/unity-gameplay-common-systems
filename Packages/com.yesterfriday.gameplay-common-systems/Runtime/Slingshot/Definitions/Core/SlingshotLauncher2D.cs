using System;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Slingshot
{
    public sealed class SlingshotLauncher2D : MonoBehaviour
    {
        [Header("Definition")]
        [SerializeField] private SlingshotDefinition2D _definition;
        public SlingshotDefinition2D Definition => _definition;

        [Header("References")]
        [SerializeField] private Camera _inputCamera;
        public Camera InputCamera => _inputCamera;

        [SerializeField] private Transform _origin;
        public Transform Origin => _origin;

        public bool IsCoolingDown => Time.time < _nextLaunchTime;
        public SlingshotFailReason LastFailReason { get; private set; }

        public event Action<Vector2, float> OnPullChanged;
        public event Action<SlingshotDefinition2D, GameObject, Vector2, float> OnLaunched;
        public event Action<SlingshotFailReason> OnLaunchFailed;
        public event Action<bool> OnCooldownChanged;

        private enum State
        {
            Idle = 0,
            Pulling = 1,
        }

        private State _state = State.Idle;

        private float _nextLaunchTime;
        private Vector3 _pullStartWorld3; // z는 origin.z 고정 규칙에 맞춰 저장
        private Vector3 _pullNowWorld3;

        private bool _lastCoolingDown;

        private void Reset()
        {
            _inputCamera = Camera.main;
        }

        private void Update()
        {
            bool nowCoolingDown = IsCoolingDown;
            if (nowCoolingDown != _lastCoolingDown)
            {
                _lastCoolingDown = nowCoolingDown;
                OnCooldownChanged?.Invoke(IsCoolingDown);
            }
        }

        // 상태 머신 API
        public bool TryBeginPull(Vector2 screenPosition)
        {
            if (_state == State.Pulling)
            {
                return Fail(SlingshotFailReason.AlreadyPulling);
            }

            if (!TryValidateCommon(out SlingshotFailReason reason))
            {
                return Fail(reason);
            }

            if (IsCoolingDown)
            {
                return Fail(SlingshotFailReason.CoolingDown);
            }

            if (!TryScreenToWorld(screenPosition, out var world))
            {
                return Fail(SlingshotFailReason.NoPlaneHit);
            }
            
            _state = State.Pulling;
            _pullNowWorld3 = world;
            LastFailReason = SlingshotFailReason.None;
            return true;
        }

        public bool TryUpdatePull(Vector2 screenPosition, out Vector2 pullWorld, out float normalizedPull)
        {
            pullWorld = default;
            normalizedPull = 0f;
            
            if (_state != State.Pulling)
            {
                return Fail(SlingshotFailReason.NotPulling);
            }

            if (!TryValidateCommon(out SlingshotFailReason reason))
            {
                _state = State.Idle;
                return Fail(reason);
            }

            if (!TryScreenToWorld(screenPosition, out var world))
            {
                return Fail(SlingshotFailReason.NoPlaneHit);
            }
            
            _pullNowWorld3 = world;
            Vector2 rawPull = new Vector3(_pullStartWorld3.x - _pullNowWorld3.x, _pullStartWorld3.y - _pullNowWorld3.y);
            pullWorld = ClampPull(rawPull, _definition.MaxPullDistance, out normalizedPull);

            OnPullChanged?.Invoke(pullWorld, normalizedPull);
            
            LastFailReason = SlingshotFailReason.None;
            return true;
        }

        public bool TryEndPull(Vector2 screenPosition, out GameObject projectile, out float impulseApplied)
        {
            projectile = null;
            impulseApplied = 0f;

            if (_state != State.Pulling)
            {
                return Fail(SlingshotFailReason.NotPulling);
            }

            if (!TryUpdatePull(screenPosition, out var pullWorld, out _))
            {
                _state = State.Idle;
                return false;
            }
            
            _state = State.Idle;
            return TryLaunch(pullWorld, out projectile, out impulseApplied);
        }

        // Launch: 실패를 한 곳에서 모아 처리
        public bool TryLaunch(Vector2 pullWorld, out GameObject projectile, out float impulseApplied)
        {
            projectile = null;
            impulseApplied = 0f;

            if (!TryValidateCommon(out var reason))
            {
                return Fail(reason);
            }

            if (IsCoolingDown)
            {
                return Fail(SlingshotFailReason.CoolingDown);
            }

            if (_definition.ProjectilePrefab == null)
            {
                return Fail(SlingshotFailReason.NullDefinition);
            }

            if (!IsValidParams(_definition))
            {
                Debug.Log($"[Slingshot] Params: MaxPullDistance={_definition.MaxPullDistance}, MinPullDistance={_definition.MinPullDistance}, MinImpulse={_definition.MinImpulse}, MaxImpulse={_definition.MaxImpulse}, Cooldown={_definition.CooldownSeconds}");
                return Fail(SlingshotFailReason.InvalidParams);
            }

            float dist = pullWorld.magnitude;
            if (dist < _definition.MaxPullDistance)
            {
                return Fail(SlingshotFailReason.PullTooSmall);
            }
            
            float normalized = Mathf.Clamp01(dist/_definition.MaxPullDistance);
            impulseApplied = Mathf.Lerp(_definition.MinImpulse, _definition.MaxImpulse, normalized);
            Vector2 dir = pullWorld.normalized;

            Vector3 spawnPos = _origin.position + (Vector3)_definition.SpawnOffset2D;
            projectile = Instantiate(_definition.ProjectilePrefab, spawnPos, Quaternion.identity);

            if (!projectile.TryGetComponent<Rigidbody2D>(out var rb))
            {
                Destroy(projectile);
                projectile = null;
                impulseApplied = 0f;
                return Fail(SlingshotFailReason.NoRigidbody2D);
            }
            
            rb.AddForce(dir * impulseApplied, ForceMode2D.Impulse);
            
            _nextLaunchTime = Time.time + _definition.CooldownSeconds;
            
            LastFailReason = SlingshotFailReason.None;
            OnLaunched?.Invoke(_definition, projectile, dir, impulseApplied);
            
            return true;
        }
        
        private static bool IsValidParams(SlingshotDefinition2D def)
        {
            // v0.1: 런타임 방어 (OnValidate가 있어도 외부 입력을 100% 신뢰하면 안 됨)

            // MaxPullDistance
            if (def.MaxPullDistance <= 0f) return false;

            // Impulse range
            if (def.MinImpulse < 0f) return false;
            if (def.MaxImpulse <= 0f) return false;
            if (def.MaxImpulse < def.MinImpulse) return false;

            // Cooldown
            if (def.CooldownSeconds < 0f) return false;

            // MinPullDistance
            if (def.MinPullDistance < 0f) return false;

            return true;
        }
        
        // 공용 Fail 처리
        private bool Fail(SlingshotFailReason reason)
        {
            LastFailReason = reason;
            OnLaunchFailed?.Invoke(reason);
            return false;
        }

        // 공용 유효성 체크
        private bool TryValidateCommon(out SlingshotFailReason reason)
        {
            if(_definition == null)
            {
                reason = SlingshotFailReason.NullDefinition;
                return false;
            }

            if (_origin == null)
            {
                reason = SlingshotFailReason.NullOrigin;
                return false;
            }

            if (_inputCamera == null)
            {
                _inputCamera = Camera.main;
            }

            if (_inputCamera == null)
            {
                reason = SlingshotFailReason.NullCamera;
            }
            
            reason = SlingshotFailReason.None;
            return true;
        }

        private bool TryScreenToWorld(Vector2 screenPos, out Vector3 world)
        {
            world = default;

            float planeZ = _origin.position.z;
            return SlingshotScreenToWorld2D.TryGetWorldOnOriginPlane(_inputCamera, screenPos, new Vector3(0, 0, planeZ),
                out world);
        }

        private static Vector2 ClampPull(Vector2 rawPull, float maxDist, out float normalized)
        {
            if (maxDist <= 0f)
            {
                normalized = 0f;
                return Vector2.zero;
            }
            
            float dist  = rawPull.magnitude;
            if (dist <= 0f)
            {
                normalized = 0f;
                return Vector2.zero;
            }

            float clampedDist =  Mathf.Clamp(dist, maxDist, 0f);
            normalized = Mathf.Clamp01(clampedDist/maxDist);
            return rawPull.normalized * clampedDist;
        }
    }
}