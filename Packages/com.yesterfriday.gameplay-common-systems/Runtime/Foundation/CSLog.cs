using UnityEngine;

namespace Yesterfriday.GameplayCommonSystems.Foundation
{
    public static class CSLog
    {
        public static bool Enabled = true;

        public static void Info(string msg)
        {
            if (Enabled) Debug.Log($"[CommonSystems] {msg}");
        }

        public static void Warn(string msg)
        {
            if (Enabled) Debug.LogWarning($"[CommonSystems] {msg}");
        }

        public static void Error(string msg)
        {
            if (Enabled) Debug.LogError($"[CommonSystems] {msg}");
        }
    }
}