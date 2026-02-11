using System;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Samples.Common.Gameplay
{
    public sealed class WaveController : MonoBehaviour
    {
        [SerializeField] private int _maxWaves = 10;
        public int MaxWaves => _maxWaves;
        public int CurrentWave {get; private set;}
        
        public bool IsRunning {get; private set;}
        
        public event Action<int> OnWaveStarted;
        public event Action<int> OnWaveEnded;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_maxWaves < 1)
            {
                Debug.LogWarning($"{nameof(WaveController)}: MaxWaves must be >= 1. Clamping to 1.", this);
                _maxWaves = 1;
            }
        }
#endif

        public bool TryStartNextWave()
        {
            if (IsRunning)
            {
                return false;
            }

            if (CurrentWave >= _maxWaves)
            {
                return false;
            }

            IsRunning = true;
            CurrentWave++;
            OnWaveStarted?.Invoke(CurrentWave);
            
            return true;
        }

        public bool TryEndWave()
        {
            if (!IsRunning)
            {
                return false;
            }
            
            IsRunning = false;
            OnWaveEnded?.Invoke(CurrentWave);
            
            return true;
        }

        public bool TryReset()
        {
            if (IsRunning)
            {
                return false;
            }

            if (CurrentWave == 0)
            {
                return false;
            }

            CurrentWave = 0;
            return true;
        }

    }
}