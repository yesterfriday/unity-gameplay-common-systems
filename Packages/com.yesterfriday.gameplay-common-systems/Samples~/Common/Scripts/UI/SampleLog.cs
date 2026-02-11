using UnityEngine;
using System;

namespace Yesterfriday.GameplayCommonSystems.Samples.Common.UI
{
    public class SampleLog : MonoBehaviour
    {
        [SerializeField] private bool _alsoToConsole = true;
        
        public event Action<string> OnLogged;

        public void Info(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            
            OnLogged?.Invoke(message);

            if (_alsoToConsole)
            {
                Debug.Log(message, this);
            }
        }

        public void Warn(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var decorated = $"[WARN] {message}";
            OnLogged?.Invoke(decorated);
            
            if (_alsoToConsole)
            {
                Debug.LogWarning(decorated, this);
            }
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            
        }
#endif
    }
}