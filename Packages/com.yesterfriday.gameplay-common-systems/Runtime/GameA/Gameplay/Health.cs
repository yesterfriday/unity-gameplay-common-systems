using System;
using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Samples.Common.Gameplay
{
    public sealed class Health : MonoBehaviour
    {
        [SerializeField] private int _max = 10;
        public int Max => _max;
        public int Current { get; private set; }

        public event Action<int, int> Changed;
        public event Action Died;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_max < 1)
            {
                Debug.LogWarning($"{nameof(Health)}: Max must be >= 1. Clamping to 1.", this);
                _max = 1;
            }
        }
#endif

        private void Awake()
        {
            Current = Mathf.Clamp(Current <= 0 ? _max : Current, 0, _max);
            Changed?.Invoke(Current, _max);
        }

        public bool TryDamage(int amount)
        {
            if (amount <= 0) return false;
            if (Current <= 0) return false;

            int prev = Current;
            Current = Mathf.Max(0, Current - amount);

            if (Current == prev) return false;

            Changed?.Invoke(Current, _max);

            if (Current == 0 && prev > 0)
                Died?.Invoke();

            return true;
        }

        public bool TryHeal(int amount)
        {
            if (amount <= 0) return false;
            if (Current <= 0) return false;

            int prev = Current;
            Current = Mathf.Min(_max, Current + amount);

            if (Current == prev) return false;

            Changed?.Invoke(Current, _max);
            return true;
        }
    }
}