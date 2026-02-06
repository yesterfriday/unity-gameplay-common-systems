using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Slingshot.Samples
{
    /// <summary>
    /// Mouse + Touch 입력을 SlingshotLauncher2D의 Begin/Update/End로 브릿지한다.
    /// - Touch가 존재하면 Touch 우선
    /// - Touch가 없으면 Mouse 사용
    /// </summary>
    public sealed class SlingshotDemoInputBridge2D : MonoBehaviour
    {
        [SerializeField] private SlingshotLauncher2D _launcher;

        [Header("Options")]
        [SerializeField] private bool _ignoreInputWhenPointerOverUI = false;

        private bool _isPointerDown;

        private void Reset()
        {
            _launcher = FindFirstObjectByType<SlingshotLauncher2D>();
        }

        private void Update()
        {
            if (_launcher == null)
                return;

            // (선택) UI 위 입력 무시가 필요하면 EventSystem 기반으로 확장 가능.
            if (_ignoreInputWhenPointerOverUI)
            {
                // 간단 버전: 여기서는 미구현(필요 시 EventSystem 의존 추가)
            }

            if (TryGetPrimaryPointer(out var screenPos, out var phase))
            {
                switch (phase)
                {
                    case PointerPhase.Down:
                        _isPointerDown = _launcher.TryBeginPull(screenPos);
                        break;

                    case PointerPhase.Move:
                        if (_isPointerDown)
                            _launcher.TryUpdatePull(screenPos, out _, out _);
                        break;

                    case PointerPhase.Up:
                        if (_isPointerDown)
                        {
                            _launcher.TryEndPull(screenPos, out _, out _);
                            _isPointerDown = false;
                        }
                        break;
                }
            }
        }

        private enum PointerPhase { None, Down, Move, Up }

        private static bool TryGetPrimaryPointer(out Vector2 screenPos, out PointerPhase phase)
        {
            screenPos = default;
            phase = PointerPhase.None;

            // Touch 우선
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                screenPos = t.position;

                switch (t.phase)
                {
                    case TouchPhase.Began: phase = PointerPhase.Down; return true;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary: phase = PointerPhase.Move; return true;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled: phase = PointerPhase.Up; return true;
                    default: return false;
                }
            }

            // Mouse
            if (Input.GetMouseButtonDown(0))
            {
                screenPos = Input.mousePosition;
                phase = PointerPhase.Down;
                return true;
            }

            if (Input.GetMouseButton(0))
            {
                screenPos = Input.mousePosition;
                phase = PointerPhase.Move;
                return true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                screenPos = Input.mousePosition;
                phase = PointerPhase.Up;
                return true;
            }

            return false;
        }
    }
}