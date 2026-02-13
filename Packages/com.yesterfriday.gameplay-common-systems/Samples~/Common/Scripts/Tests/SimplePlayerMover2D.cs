using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.SamplesCommon.Tests
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class SimplePlayerMover2D : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 6f;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
        }

        private void FixedUpdate()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");

            Vector2 dir = new Vector2(x, y).normalized;
            _rb.velocity = dir * _moveSpeed;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_moveSpeed < 0f) _moveSpeed = 0f;
        }
#endif
    }
}