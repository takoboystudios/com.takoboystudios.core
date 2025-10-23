using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Extension methods for float providing utilities for range checking, angle operations, and conversions.
    /// </summary>
    public static class FloatExtensions
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
        /// float health = 75f;
        /// if (health.IsInRangeInclusive(0f, 100f))
        /// {
        ///     Debug.Log("Health is valid");
        /// }
        /// </code>
        /// </example>
        public static bool IsInRangeInclusive(this float value, float min, float max)
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
        public static bool IsInRangeExclusive(this float value, float min, float max)
        {
            return value > min && value < max;
        }

        /// <summary>
        /// Checks if the value is approximately equal to another value within a small tolerance.
        /// </summary>
        /// <param name="value">The first value</param>
        /// <param name="other">The second value</param>
        /// <param name="tolerance">The tolerance for comparison (default: 0.0001f)</param>
        /// <returns>True if the values are approximately equal</returns>
        public static bool IsApproximately(this float value, float other, float tolerance = 0.0001f)
        {
            return Mathf.Abs(value - other) < tolerance;
        }

        /// <summary>
        /// Checks if the value is approximately zero within a small tolerance.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <param name="tolerance">The tolerance for comparison (default: 0.0001f)</param>
        /// <returns>True if the value is approximately zero</returns>
        public static bool IsApproximatelyZero(this float value, float tolerance = 0.0001f)
        {
            return Mathf.Abs(value) < tolerance;
        }

        #endregion

        #region Angle Operations

        /// <summary>
        /// Normalizes an angle to the range [0, 360).
        /// </summary>
        /// <param name="angle">The angle in degrees</param>
        /// <returns>The normalized angle in the range [0, 360)</returns>
        /// <remarks>
        /// Examples:
        /// - 365° becomes 5°
        /// - -5° becomes 355°
        /// - 720° becomes 0°
        /// </remarks>
        /// <example>
        /// <code>
        /// float angle = 365f;
        /// float normalized = angle.NormalizeAngle(); // Returns 5f
        /// </code>
        /// </example>
        public static float NormalizeAngle(this float angle)
        {
            angle = angle % 360f;
            
            if (angle < 0f)
                angle += 360f;
            
            return angle;
        }

        /// <summary>
        /// Normalizes an angle to the range [-180, 180).
        /// </summary>
        /// <param name="angle">The angle in degrees</param>
        /// <returns>The normalized angle in the range [-180, 180)</returns>
        /// <remarks>
        /// This is useful for calculating signed angular differences.
        /// Examples:
        /// - 190° becomes -170°
        /// - -190° becomes 170°
        /// - 365° becomes 5°
        /// </remarks>
        public static float NormalizeAngleSigned(this float angle)
        {
            angle = angle.NormalizeAngle();
            
            if (angle > 180f)
                angle -= 360f;
            
            return angle;
        }

        /// <summary>
        /// Clamps an angle between minimum and maximum angle values (in degrees).
        /// </summary>
        /// <param name="angle">The angle to clamp</param>
        /// <param name="min">The minimum angle</param>
        /// <param name="max">The maximum angle</param>
        /// <returns>The clamped angle</returns>
        /// <remarks>
        /// The angle is normalized before clamping to ensure proper comparison.
        /// </remarks>
        public static float ClampAngle(this float angle, float min, float max)
        {
            float normalizedAngle = angle.NormalizeAngle();
            return Mathf.Clamp(normalizedAngle, min, max);
        }

        /// <summary>
        /// Gets the shortest angular difference between two angles.
        /// </summary>
        /// <param name="from">The starting angle in degrees</param>
        /// <param name="to">The target angle in degrees</param>
        /// <returns>The shortest angular difference in the range [-180, 180]</returns>
        /// <remarks>
        /// Positive values indicate clockwise rotation, negative values indicate counter-clockwise.
        /// </remarks>
        public static float DeltaAngle(this float from, float to)
        {
            return Mathf.DeltaAngle(from, to);
        }

        #endregion

        #region Angle Conversion

        /// <summary>
        /// Converts an angle from degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees</param>
        /// <returns>The angle in radians</returns>
        /// <example>
        /// <code>
        /// float degrees = 180f;
        /// float radians = degrees.ToRadians(); // Returns π (approximately 3.14159)
        /// </code>
        /// </example>
        public static float ToRadians(this float degrees)
        {
            return degrees * Mathf.Deg2Rad;
        }

        /// <summary>
        /// Converts an angle from radians to degrees.
        /// </summary>
        /// <param name="radians">The angle in radians</param>
        /// <returns>The angle in degrees</returns>
        /// <example>
        /// <code>
        /// float radians = Mathf.PI;
        /// float degrees = radians.ToDegrees(); // Returns 180f
        /// </code>
        /// </example>
        public static float ToDegrees(this float radians)
        {
            return radians * Mathf.Rad2Deg;
        }

        #endregion

        #region Mathematical Operations

        /// <summary>
        /// Returns the value raised to the specified power.
        /// </summary>
        /// <param name="value">The base value</param>
        /// <param name="exponent">The exponent</param>
        /// <returns>The result of value^exponent</returns>
        public static float Pow(this float value, float exponent)
        {
            return Mathf.Pow(value, exponent);
        }

        /// <summary>
        /// Returns the square of the value.
        /// </summary>
        /// <param name="value">The value to square</param>
        /// <returns>value * value</returns>
        public static float Sqr(this float value)
        {
            return value * value;
        }

        /// <summary>
        /// Returns the square root of the value.
        /// </summary>
        /// <param name="value">The value (must be non-negative)</param>
        /// <returns>The square root of the value</returns>
        public static float Sqrt(this float value)
        {
            return Mathf.Sqrt(value);
        }

        /// <summary>
        /// Returns the absolute value.
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The absolute value</returns>
        public static float Abs(this float value)
        {
            return Mathf.Abs(value);
        }

        /// <summary>
        /// Returns the sign of the value (-1, 0, or 1).
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>-1 if negative, 1 if positive, 0 if zero</returns>
        public static float Sign(this float value)
        {
            return Mathf.Sign(value);
        }

        /// <summary>
        /// Rounds the value to the nearest integer.
        /// </summary>
        /// <param name="value">The value to round</param>
        /// <returns>The rounded value</returns>
        public static float Round(this float value)
        {
            return Mathf.Round(value);
        }

        /// <summary>
        /// Rounds the value to the specified number of decimal places.
        /// </summary>
        /// <param name="value">The value to round</param>
        /// <param name="decimals">The number of decimal places</param>
        /// <returns>The rounded value</returns>
        public static float Round(this float value, int decimals)
        {
            float multiplier = Mathf.Pow(10f, decimals);
            return Mathf.Round(value * multiplier) / multiplier;
        }

        /// <summary>
        /// Returns the largest integer less than or equal to the value.
        /// </summary>
        /// <param name="value">The value to floor</param>
        /// <returns>The floored value</returns>
        public static float Floor(this float value)
        {
            return Mathf.Floor(value);
        }

        /// <summary>
        /// Returns the smallest integer greater than or equal to the value.
        /// </summary>
        /// <param name="value">The value to ceil</param>
        /// <returns>The ceiled value</returns>
        public static float Ceil(this float value)
        {
            return Mathf.Ceil(value);
        }

        #endregion

        #region Clamping & Remapping

        /// <summary>
        /// Clamps the value between 0 and 1.
        /// </summary>
        /// <param name="value">The value to clamp</param>
        /// <returns>The clamped value in range [0, 1]</returns>
        public static float Clamp01(this float value)
        {
            return Mathf.Clamp01(value);
        }

        /// <summary>
        /// Clamps the value between min and max.
        /// </summary>
        /// <param name="value">The value to clamp</param>
        /// <param name="min">The minimum value</param>
        /// <param name="max">The maximum value</param>
        /// <returns>The clamped value in range [min, max]</returns>
        public static float Clamp(this float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        /// <param name="value">The value to remap</param>
        /// <param name="fromMin">The minimum of the input range</param>
        /// <param name="fromMax">The maximum of the input range</param>
        /// <param name="toMin">The minimum of the output range</param>
        /// <param name="toMax">The maximum of the output range</param>
        /// <returns>The remapped value</returns>
        /// <example>
        /// <code>
        /// // Remap 5 from range [0, 10] to range [0, 100]
        /// float remapped = 5f.Remap(0f, 10f, 0f, 100f); // Returns 50f
        /// </code>
        /// </example>
        public static float Remap(this float value, float fromMin, float fromMax, float toMin, float toMax)
        {
            float t = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, t);
        }

        /// <summary>
        /// Wraps a value to stay within a specified range.
        /// </summary>
        /// <param name="value">The value to wrap</param>
        /// <param name="min">The minimum value of the range</param>
        /// <param name="max">The maximum value of the range</param>
        /// <returns>The wrapped value</returns>
        /// <remarks>
        /// Unlike clamping, wrapping causes values outside the range to loop back.
        /// For example, with range [0, 10]: 11 becomes 1, -1 becomes 9.
        /// </remarks>
        public static float Wrap(this float value, float min, float max)
        {
            float range = max - min;
            
            if (range == 0f)
                return min;
            
            return min + ((value - min) % range + range) % range;
        }

        #endregion

        #region Interpolation

        /// <summary>
        /// Linearly interpolates between two values.
        /// </summary>
        /// <param name="from">The start value</param>
        /// <param name="to">The end value</param>
        /// <param name="t">The interpolation value (0-1)</param>
        /// <returns>The interpolated value</returns>
        public static float Lerp(this float from, float to, float t)
        {
            return Mathf.Lerp(from, to, t);
        }

        /// <summary>
        /// Calculates the inverse lerp (what t value would lerp from 'from' to 'to' to get 'value').
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="from">The start value</param>
        /// <param name="to">The end value</param>
        /// <returns>The t value (0-1) where lerp(from, to, t) = value</returns>
        public static float InverseLerp(this float value, float from, float to)
        {
            return Mathf.InverseLerp(from, to, value);
        }

        #endregion

        #region Legacy Support (Obsolete)

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees</param>
        /// <returns>The angle in radians</returns>
        [System.Obsolete("Use ToRadians() instead for consistent naming.")]
        public static float Deg2Rad(this float degrees)
        {
            return ToRadians(degrees);
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The angle in degrees</param>
        /// <returns>The angle in radians</returns>
        [System.Obsolete("Use ToRadians() instead.")]
        public static float DegreesToRadians(this float degrees)
        {
            return ToRadians(degrees);
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radians">The angle in radians</param>
        /// <returns>The angle in degrees</returns>
        [System.Obsolete("Use ToDegrees() instead for consistent naming.")]
        public static float Rad2Deg(this float radians)
        {
            return ToDegrees(radians);
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        /// <param name="radians">The angle in radians</param>
        /// <returns>The angle in degrees</returns>
        [System.Obsolete("Use ToDegrees() instead.")]
        public static float RadiansToDegrees(this float radians)
        {
            return ToDegrees(radians);
        }

        #endregion
    }
}
