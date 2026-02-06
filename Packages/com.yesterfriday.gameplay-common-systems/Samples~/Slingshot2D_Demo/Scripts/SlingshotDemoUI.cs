using System.Text;
using TMPro;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Slingshot.Samples
{
    public sealed class SlingshotDemoUI : MonoBehaviour
    {
        [SerializeField] private SlingshotLauncher2D _launcher;

        [Header("UI (TextMeshPro)")]
        [SerializeField] private TMP_Text _text;

        private Vector2 _lastPull;
        private float _lastNormalized;
        private float _lastImpulse;

        private void Reset()
        {
            _launcher = FindFirstObjectByType<SlingshotLauncher2D>();
            _text = GetComponent<TMP_Text>();
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

        private void Update()
        {
            if (_launcher == null || _text == null) return;

            var sb = new StringBuilder(256);

            sb.AppendLine("Slingshot2D Demo");
            sb.Append("CoolingDown: ").AppendLine(_launcher.IsCoolingDown ? "YES" : "NO");
            sb.Append("LastFailReason: ").AppendLine(_launcher.LastFailReason.ToString());

            sb.Append("Pull: ").Append(_lastPull.ToString("F3"))
              .Append("  mag=").Append(_lastPull.magnitude.ToString("F3"))
              .AppendLine();

            sb.Append("Normalized: ").AppendLine(_lastNormalized.ToString("F3"));
            sb.Append("LastImpulse: ").AppendLine(_lastImpulse.ToString("F3"));

            _text.text = sb.ToString();
        }

        private void HandlePullChanged(Vector2 pullWorld, float normalizedPull)
        {
            _lastPull = pullWorld;
            _lastNormalized = normalizedPull;
        }

        private void HandleLaunched(SlingshotDefinition2D def, GameObject projectile, Vector2 direction, float impulse)
        {
            _lastImpulse = impulse;
        }

        private void HandleLaunchFailed(SlingshotFailReason reason)
        {
            // 필요하면 여기에 로그/이펙트 추가 가능
        }
    }
}