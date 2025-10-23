using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Extension methods for int providing utilities for range checking and common operations.
    /// </summary>
    public static class IntExtensions
    {
        #region Range Checking

        /// <summary>
        /// Checks if the value is within the specified range, including the boundaries.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="min">The minimum value (inclusive)</param>
        /// <param name="max">The maximum value (inclusive)</param>
        /// <returns>True if min ≤ value ≤ max</returns>
        /// <example>
        /// <code>
        /// int score = 75;
        /// if (score.IsInRangeInclusive(0, 100))
        /// {
        ///     Debug.Log("Valid score");
        /// }
        /// </code>
        /// </example>
        public static bool IsInRangeInclusive(this int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Checks if the value is within the specified range, excluding the boundaries.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="min">The minimum value (exclusive)</param>
        /// <param name="max">The maximum value (exclusive)</param>
        /// <returns>True if min &lt; value &lt; max</returns>
        public static bool IsInRangeExclusive(this int value, int min, int max)
        {
            return value > min && value < max;
        }

        #endregion

        #region Mathematical Operations

        /// <summary>
        /// Returns the absolute value.
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The absolute value</returns>
        public static int Abs(this int value)
        {
            return Mathf.Abs(value);
        }

        /// <summary>
        /// Returns the sign of the value (-1, 0, or 1).
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>-1 if negative, 1 if positive, 0 if zero</returns>
        public static int Sign(this int value)
        {
            if (value < 0) return -1;
            if (value > 0) return 1;
            return 0;
        }

        /// <summary>
        /// Returns the square of the value.
        /// </summary>
        /// <param name="value">The value to square</param>
        /// <returns>value * value</returns>
        public static int Sqr(this int value)
        {
            return value * value;
        }

        /// <summary>
        /// Returns the value raised to the specified power.
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="exponent">The exponent</param>
        /// <returns>The result of value^exponent</returns>
        public static int Pow(this int value, int exponent)
        {
            if (exponent < 0)
            {
                Debug.LogWarning("[IntExtensions] Negative exponents not supported for integer power. Returning 0.");
                return 0;
            }

            if (exponent == 0)
                return 1;

            int result = 1;
            for (int i = 0; i < exponent; i++)
            {
                result *= value;
            }

            return result;
        }

        #endregion

        #region Clamping & Wrapping

        /// <summary>
        /// Clamps the value between min and max.
        /// </summary>
        /// <param name="value">The value to clamp</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>The clamped value in range [min, max]</returns>
        public static int Clamp(this int value, int min, int max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// Clamps the value between 0 and max.
        /// </summary>
        /// <param name="value">The value to clamp</param>
        /// <param name="max">The maximum value</param>
        /// <returns>The clamped value in range [0, max]</returns>
        public static int ClampPositive(this int value, int max)
        {
            return Mathf.Clamp(value, 0, max);
        }

        /// <summary>
        /// Wraps a value to stay within a specified range.
        /// </summary>
        /// <param name="value">The value to wrap</param>
        /// <param name="min">The minimum value of the range</param>
        /// <param name="max">The maximum value of the range (exclusive)</param>
        /// <returns>The wrapped value</returns>
        /// <remarks>
        /// Unlike clamping, wrapping causes values outside the range to loop back.
        /// For example, with range [0, 10): 10 becomes 0, 11 becomes 1, -1 becomes 9.
        /// Note: max is exclusive, so valid range is [min, max).
        /// </remarks>
        /// <example>
        /// <code>
        /// int index = 10;
        /// int wrapped = index.Wrap(0, 10); // Returns 0
        /// 
        /// index = -1;
        /// wrapped = index.Wrap(0, 10); // Returns 9
        /// </code>
        /// </example>
        public static int Wrap(this int value, int min, int max)
        {
            int range = max - min;
            
            if (range <= 0)
                return min;
            
            return min + ((value - min) % range + range) % range;
        }

        #endregion

        #region Number Properties

        /// <summary>
        /// Checks if the number is even.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the number is even</returns>
        public static bool IsEven(this int value)
        {
            return value % 2 == 0;
        }

        /// <summary>
        /// Checks if the number is odd.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the number is odd</returns>
        public static bool IsOdd(this int value)
        {
            return value % 2 != 0;
        }

        /// <summary>
        /// Checks if the number is positive (greater than zero).
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the number is positive</returns>
        public static bool IsPositive(this int value)
        {
            return value > 0;
        }

        /// <summary>
        /// Checks if the number is negative (less than zero).
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the number is negative</returns>
        public static bool IsNegative(this int value)
        {
            return value < 0;
        }

        /// <summary>
        /// Checks if the number is zero.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the number is zero</returns>
        public static bool IsZero(this int value)
        {
            return value == 0;
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Converts the integer to a float.
        /// </summary>
        /// <param name="value">The integer value</param>
        /// <returns>The float representation</returns>
        public static float ToFloat(this int value)
        {
            return (float)value;
        }

        /// <summary>
        /// Converts the integer to a string with optional formatting.
        /// </summary>
        /// <param name="value">The integer value</param>
        /// <param name="format">The format string (e.g., "N0" for number with commas)</param>
        /// <returns>The formatted string</returns>
        public static string ToString(this int value, string format)
        {
            return value.ToString(format);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Returns the smaller of two integers.
        /// </summary>
        /// <param name="value">The first value</param>
        /// <param name="other">The second value</param>
        /// <returns>The smaller value</returns>
        public static int Min(this int value, int other)
        {
            return Mathf.Min(value, other);
        }

        /// <summary>
        /// Returns the larger of two integers.
        /// </summary>
        /// <param name="value">The first value</param>
        /// <param name="other">The second value</param>
        /// <returns>The larger value</returns>
        public static int Max(this int value, int other)
        {
            return Mathf.Max(value, other);
        }

        /// <summary>
        /// Returns the value clamped to be at least the specified minimum.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="min">The minimum value</param>
        /// <returns>The value if it's >= min, otherwise min</returns>
        public static int AtLeast(this int value, int min)
        {
            return value < min ? min : value;
        }

        /// <summary>
        /// Returns the value clamped to be at most the specified maximum.
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>The value if it's <= max, otherwise max</returns>
        public static int AtMost(this int value, int max)
        {
            return value > max ? max : value;
        }

        /// <summary>
        /// Repeats an action the specified number of times.
        /// </summary>
        /// <param name="count">The number of times to repeat</param>
        /// <param name="action">The action to perform (receives the current index)</param>
        /// <example>
        /// <code>
        /// 5.Times(i => Debug.Log($"Iteration {i}"));
        /// // Logs: "Iteration 0", "Iteration 1", ... "Iteration 4"
        /// </code>
        /// </example>
        public static void Times(this int count, System.Action<int> action)
        {
            if (action == null)
            {
                Debug.LogWarning("[IntExtensions] Cannot execute null action");
                return;
            }

            if (count < 0)
            {
                Debug.LogWarning($"[IntExtensions] Cannot repeat action negative times: {count}");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                action(i);
            }
        }

        /// <summary>
        /// Repeats an action the specified number of times.
        /// </summary>
        /// <param name="count">The number of times to repeat</param>
        /// <param name="action">The action to perform</param>
        /// <example>
        /// <code>
        /// 3.Times(() => Debug.Log("Hello"));
        /// // Logs "Hello" three times
        /// </code>
        /// </example>
        public static void Times(this int count, System.Action action)
        {
            if (action == null)
            {
                Debug.LogWarning("[IntExtensions] Cannot execute null action");
                return;
            }

            if (count < 0)
            {
                Debug.LogWarning($"[IntExtensions] Cannot repeat action negative times: {count}");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                action();
            }
        }

        #endregion
    }
}
