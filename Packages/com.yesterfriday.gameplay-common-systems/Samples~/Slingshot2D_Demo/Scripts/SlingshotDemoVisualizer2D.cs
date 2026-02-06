using System.Collections.Generic;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Slingshot.Samples
{
    public sealed class SlingshotDemoVisualizer2D : MonoBehaviour
    {
        [SerializeField] private SlingshotLauncher2D _launcher;

        [Header("Line Renderer (Pull Only)")]
        [SerializeField] private LineRenderer _pullLine;

        [Header("Pull Line (Bar Style)")]
        [SerializeField] private float _lineWidth = 0.08f;

        [Header("Emphasis (dramatic via length/color only)")]
        [Range(0.1f, 2f)] [SerializeField] private float _emphasisExponent = 0.45f;

        [SerializeField] private Color _weakColor = new Color(0.25f, 0.85f, 1f, 0.5f);
        [SerializeField] private Color _strongColor = new Color(1f, 0.25f, 0.25f, 1f);

        [Header("Trajectory Preview")]
        [SerializeField] private bool _showTrajectory = true;
        [Min(2)] [SerializeField] private int _trajectorySteps = 18;
        [Min(0.01f)] [SerializeField] private float _trajectoryDt = 0.08f;

        [SerializeField] private Transform _trajectoryDotPrefab;
        [SerializeField] private Transform _trajectoryDotsRoot;

        [Header("Dot (Fixed Local Size)")]
        [SerializeField] private float _dotLocalScale = 0.12f; // 고정 localScale
        [SerializeField] private float _dotAlpha = 0.75f;      // 고정 알파

        private readonly List<Transform> _dots = new();

        private void Reset()
        {
            _launcher = FindFirstObjectByType<SlingshotLauncher2D>();
        }

        private void Awake()
        {
            if (_trajectoryDotsRoot == null)
            {
                _trajectoryDotsRoot = new GameObject("TrajectoryDots").transform;
                _trajectoryDotsRoot.SetParent(transform, false);
            }

            ConfigureBarLine(_pullLine);
        }

        private void OnEnable()
        {
            if (_launcher == null) return;
            _launcher.OnPullChanged += HandlePullChanged;
            _launcher.OnLaunched += HandleLaunched;
            _launcher.OnLaunchFailed += HandleLaunchFailed;
        }

        private void OnDisable()
        {
            if (_launcher == null) return;
            _launcher.OnPullChanged -= HandlePullChanged;
            _launcher.OnLaunched -= HandleLaunched;
            _launcher.OnLaunchFailed -= HandleLaunchFailed;
        }

        private void HandlePullChanged(Vector2 pullWorld, float normalizedPull)
        {
            if (_launcher.Origin == null) return;

            float n = Mathf.Clamp01(normalizedPull);
            float nEmph = Mathf.Pow(n, _emphasisExponent);

            ApplyLineColor(_pullLine, nEmph);

            Vector3 origin = _launcher.Origin.position;

            // Pull line: origin -> hand(데모용)
            Vector3 hand = origin - (Vector3)pullWorld;
            DrawLine(_pullLine, origin, hand);

            if (_showTrajectory)
                UpdateTrajectoryPreview(origin, pullWorld);
            else
                SetDotsActive(false);
        }

        private void HandleLaunched(SlingshotDefinition2D def, GameObject projectile, Vector2 direction, float impulse)
            => HideAll();

        private void HandleLaunchFailed(SlingshotFailReason reason)
            => HideAll();

        private void HideAll()
        {
            SetLineActive(_pullLine, false);
            SetDotsActive(false);
        }

        // ===== Pull line setup =====
        private void ConfigureBarLine(LineRenderer lr)
        {
            if (lr == null) return;

            lr.enabled = false;
            lr.positionCount = 0;

            // 막대 느낌: 둥근 캡/코너 제거
            lr.numCapVertices = 0;
            lr.numCornerVertices = 0;

            // 굵기 고정
            lr.startWidth = _lineWidth;
            lr.endWidth = _lineWidth;

            lr.textureMode = LineTextureMode.Stretch;
        }

        private void ApplyLineColor(LineRenderer lr, float nEmph)
        {
            if (lr == null) return;

            Color c = Color.Lerp(_weakColor, _strongColor, Mathf.Clamp01(nEmph));
            lr.startColor = c;
            lr.endColor = c;

            // 혹시 인스펙터에서 바뀌었을 수 있으니 고정 재적용
            lr.startWidth = _lineWidth;
            lr.endWidth = _lineWidth;
        }

        // ===== Trajectory preview =====
        private void UpdateTrajectoryPreview(Vector3 origin, Vector2 pullWorld)
        {
            if (_launcher.Definition == null || _trajectoryDotPrefab == null)
            {
                SetDotsActive(false);
                return;
            }

            var def = _launcher.Definition;
            if (def.MaxPullDistance <= 0f)
            {
                SetDotsActive(false);
                return;
            }

            // Launcher와 동일한 impulse 계산
            float dist = pullWorld.magnitude;
            float t = Mathf.Clamp01(dist / def.MaxPullDistance);
            float impulse = Mathf.Lerp(def.MinImpulse, def.MaxImpulse, t);

            Vector2 dir = pullWorld.sqrMagnitude > 0f ? pullWorld.normalized : Vector2.right;
            Vector2 v0 = dir * impulse;

            Vector2 g = Physics2D.gravity;

            EnsureDots(_trajectorySteps);

            for (int i = 0; i < _trajectorySteps; i++)
            {
                float time = i * _trajectoryDt;
                Vector2 p = (Vector2)origin + v0 * time + 0.5f * g * (time * time);

                Transform d = _dots[i];
                d.position = new Vector3(p.x, p.y, origin.z);
                
                d.localScale = Vector3.one * _dotLocalScale;
                ApplyDotAlpha(d, _dotAlpha);

                d.gameObject.SetActive(true);
            }

            for (int i = _trajectorySteps; i < _dots.Count; i++)
                _dots[i].gameObject.SetActive(false);
        }

        private void EnsureDots(int count)
        {
            while (_dots.Count < count)
            {
                var dot = Instantiate(_trajectoryDotPrefab, _trajectoryDotsRoot);
                dot.gameObject.SetActive(false);
                _dots.Add(dot);
            }
        }

        private static void ApplyDotAlpha(Transform dot, float alpha)
        {
            if (dot.TryGetComponent<SpriteRenderer>(out var sr))
            {
                var c = sr.color;
                c.a = Mathf.Clamp01(alpha);
                sr.color = c;
            }
        }

        private static void DrawLine(LineRenderer lr, Vector3 a, Vector3 b)
        {
            if (lr == null) return;
            lr.enabled = true;
            lr.positionCount = 2;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);
        }

        private static void SetLineActive(LineRenderer lr, bool active)
        {
            if (lr == null) return;
            lr.enabled = active;
        }

        private void SetDotsActive(bool active)
        {
            for (int i = 0; i < _dots.Count; i++)
                _dots[i].gameObject.SetActive(active);
        }
    }
}