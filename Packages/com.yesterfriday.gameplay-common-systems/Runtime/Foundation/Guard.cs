using System;

namespace Yesterfriday.GameplayCommonSystems.Foundation
{
    public static class Guard
    {
        public static void NotNull(object value, string paramName)
        {
            if (value == null) throw new ArgumentNullException(paramName);
        }

        public static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}