using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Slingshot.Samples
{
    public sealed class SlingshotDemoAutoDestroy : MonoBehaviour
    {
        [SerializeField] private float _lifetimeSeconds = 5f;

        private void OnEnable()
        {
            Destroy(gameObject, _lifetimeSeconds);
        }
    }
}