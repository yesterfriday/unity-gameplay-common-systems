using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Samples.Common.Core
{
    public sealed class SampleBootstrap : MonoBehaviour
    {
        [SerializeField] private int _targetFrameRate = 60;
        [SerializeField] private bool _dontDestroyOnLoad;

        private void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (_targetFrameRate > 0)
            {
                Application.targetFrameRate = _targetFrameRate;
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_targetFrameRate < 0)
            {
                Debug.LogWarning($"{nameof(SampleBootstrap)}: TargetFrameRate is negative. Clamping to 0.", this);
                _targetFrameRate = 0;
            }
        }
#endif
    }
    
}