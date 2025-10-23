using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Extension methods for Vector3 providing additional utility functions for common vector operations.
    /// </summary>
    public static class Vector3Extensions
    {
        #region Component Setters

        /// <summary>
        /// Returns a new Vector3 with the X component set to the specified value.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="x">The new X value</param>
        /// <returns>A new Vector3 with the modified X component</returns>
        /// <example>
        /// <code>
        /// Vector3 pos = new Vector3(1, 2, 3);
        /// Vector3 newPos = pos.SetX(5); // Results in (5, 2, 3)
        /// </code>
        /// </example>
        public static Vector3 SetX(this Vector3 vector, float x)
        {
            return new Vector3(x, vector.y, vector.z);
        }

        /// <summary>
        /// Returns a new Vector3 with the Y component set to the specified value.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="y">The new Y value</param>
        /// <returns>A new Vector3 with the modified Y component</returns>
        /// <example>
        /// <code>
        /// Vector3 pos = new Vector3(1, 2, 3);
        /// Vector3 newPos = pos.SetY(5); // Results in (1, 5, 3)
        /// </code>
        /// </example>
        public static Vector3 SetY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, y, vector.z);
        }

        /// <summary>
        /// Returns a new Vector3 with the Z component set to the specified value.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="z">The new Z value</param>
        /// <returns>A new Vector3 with the modified Z component</returns>
        /// <example>
        /// <code>
        /// Vector3 pos = new Vector3(1, 2, 3);
        /// Vector3 newPos = pos.SetZ(5); // Results in (1, 2, 5)
        /// </code>
        /// </example>
        public static Vector3 SetZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// Returns a new Vector3 with multiple components set at once.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="x">The new X value (null to keep current)</param>
        /// <param name="y">The new Y value (null to keep current)</param>
        /// <param name="z">The new Z value (null to keep current)</param>
        /// <returns>A new Vector3 with the specified components modified</returns>
        public static Vector3 Set(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3(
                x ?? vector.x,
                y ?? vector.y,
                z ?? vector.z
            );
        }

        #endregion

        #region Component Addition

        /// <summary>
        /// Returns a new Vector3 with a value added to the X component.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="x">The value to add to X</param>
        /// <returns>A new Vector3 with the modified X component</returns>
        public static Vector3 AddX(this Vector3 vector, float x)
        {
            return new Vector3(vector.x + x, vector.y, vector.z);
        }

        /// <summary>
        /// Returns a new Vector3 with a value added to the Y component.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="y">The value to add to Y</param>
        /// <returns>A new Vector3 with the modified Y component</returns>
        public static Vector3 AddY(this Vector3 vector, float y)
        {
            return new Vector3(vector.x, vector.y + y, vector.z);
        }

        /// <summary>
        /// Returns a new Vector3 with a value added to the Z component.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="z">The value to add to Z</param>
        /// <returns>A new Vector3 with the modified Z component</returns>
        public static Vector3 AddZ(this Vector3 vector, float z)
        {
            return new Vector3(vector.x, vector.y, vector.z + z);
        }

        #endregion

        #region Conversions

        /// <summary>
        /// Converts a Vector3 to Vector2 using the X and Y components.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <returns>A Vector2 with (x, y) from the original vector</returns>
        /// <remarks>
        /// Useful for 2D operations in a 3D space, such as UI positioning or top-down games.
        /// </remarks>
        public static Vector2 ToVector2XY(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        /// <summary>
        /// Converts a Vector3 to Vector2 using the X and Z components.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <returns>A Vector2 with (x, z) from the original vector</returns>
        /// <remarks>
        /// Useful for horizontal plane calculations in 3D space, such as top-down movement
        /// where Y is the vertical axis.
        /// </remarks>
        public static Vector2 ToVector2XZ(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        /// <summary>
        /// Converts a Vector3 to Vector2 using the Y and Z components.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <returns>A Vector2 with (y, z) from the original vector</returns>
        public static Vector2 ToVector2YZ(this Vector3 vector)
        {
            return new Vector2(vector.y, vector.z);
        }

        #endregion

        #region Distance & Magnitude

        /// <summary>
        /// Calculates the squared distance between two vectors.
        /// </summary>
        /// <param name="vector">The first vector</param>
        /// <param name="other">The second vector</param>
        /// <returns>The squared distance between the vectors</returns>
        /// <remarks>
        /// Squared distance is more efficient than actual distance when you only need
        /// to compare distances, as it avoids the expensive square root calculation.
        /// Use this for distance comparisons instead of Vector3.Distance when possible.
        /// </remarks>
        public static float SqrDistance(this Vector3 vector, Vector3 other)
        {
            return (vector - other).sqrMagnitude;
        }

        /// <summary>
        /// Calculates the actual distance between two vectors.
        /// </summary>
        /// <param name="vector">The first vector</param>
        /// <param name="other">The second vector</param>
        /// <returns>The distance between the vectors</returns>
        /// <remarks>
        /// This is a convenience method equivalent to Vector3.Distance.
        /// Consider using SqrDistance for performance-critical comparisons.
        /// </remarks>
        public static float Distance(this Vector3 vector, Vector3 other)
        {
            return Vector3.Distance(vector, other);
        }

        #endregion

        #region Mathematical Operations

        /// <summary>
        /// Returns a vector perpendicular to the given vector.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <returns>A normalized perpendicular vector</returns>
        /// <remarks>
        /// The perpendicular vector is calculated using the cross product properties.
        /// Note: This method assumes the input vector is not zero.
        /// </remarks>
        public static Vector3 GetPerpendicularVector(this Vector3 vector)
        {
            // Handle near-zero components to avoid division issues
            if (Mathf.Approximately(vector.z, 0f))
            {
                return new Vector3(-vector.y, vector.x, 0f).normalized;
            }

            float z = -(vector.x + vector.y) / vector.z;
            return new Vector3(1, 1, z).normalized;
        }

        /// <summary>
        /// Clamps each component of the vector between corresponding min and max values.
        /// </summary>
        /// <param name="vector">The vector to clamp</param>
        /// <param name="min">The minimum values for each component</param>
        /// <param name="max">The maximum values for each component</param>
        /// <returns>A new vector with clamped components</returns>
        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            return new Vector3(
                Mathf.Clamp(vector.x, min.x, max.x),
                Mathf.Clamp(vector.y, min.y, max.y),
                Mathf.Clamp(vector.z, min.z, max.z)
            );
        }

        /// <summary>
        /// Returns the absolute value of each component.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <returns>A new vector with absolute values</returns>
        public static Vector3 Abs(this Vector3 vector)
        {
            return new Vector3(
                Mathf.Abs(vector.x),
                Mathf.Abs(vector.y),
                Mathf.Abs(vector.z)
            );
        }

        /// <summary>
        /// Inverts the vector (multiplies by -1).
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <returns>The inverted vector</returns>
        public static Vector3 Invert(this Vector3 vector)
        {
            return -vector;
        }

        #endregion

        #region Direction Checking

        /// <summary>
        /// Checks if the direction vector points to the right relative to a reference right vector.
        /// </summary>
        /// <param name="direction">The direction vector to check</param>
        /// <param name="right">The reference right vector (typically Transform.right)</param>
        /// <returns>True if the direction is pointing right (within 45 degrees)</returns>
        /// <remarks>
        /// Uses dot product to determine if the angle between vectors is less than 45 degrees.
        /// </remarks>
        public static bool IsRight(this Vector3 direction, Vector3 right)
        {
            float dotProduct = Vector3.Dot(right.normalized, direction.normalized);
            return dotProduct >= Mathf.Cos(45f * Mathf.Deg2Rad);
        }

        /// <summary>
        /// Checks if the direction vector points to the left relative to a reference right vector.
        /// </summary>
        /// <param name="direction">The direction vector to check</param>
        /// <param name="right">The reference right vector (typically Transform.right)</param>
        /// <returns>True if the direction is pointing left (within 45 degrees of opposite)</returns>
        public static bool IsLeft(this Vector3 direction, Vector3 right)
        {
            float dotProduct = Vector3.Dot(right.normalized, direction.normalized);
            return dotProduct <= Mathf.Cos(135f * Mathf.Deg2Rad);
        }

        /// <summary>
        /// Checks if the direction vector points laterally (either left or right).
        /// </summary>
        /// <param name="direction">The direction vector to check</param>
        /// <param name="right">The reference right vector (typically Transform.right)</param>
        /// <returns>True if the direction is pointing left or right</returns>
        public static bool IsLateral(this Vector3 direction, Vector3 right)
        {
            return direction.IsRight(right) || direction.IsLeft(right);
        }

        /// <summary>
        /// Checks if the direction vector points forward relative to a reference forward vector.
        /// </summary>
        /// <param name="direction">The direction vector to check</param>
        /// <param name="forward">The reference forward vector (typically Transform.forward)</param>
        /// <returns>True if the direction is pointing forward (within 45 degrees)</returns>
        public static bool IsForward(this Vector3 direction, Vector3 forward)
        {
            float dotProduct = Vector3.Dot(forward.normalized, direction.normalized);
            return dotProduct >= Mathf.Cos(45f * Mathf.Deg2Rad);
        }

        /// <summary>
        /// Checks if the direction vector points backward relative to a reference forward vector.
        /// </summary>
        /// <param name="direction">The direction vector to check</param>
        /// <param name="forward">The reference forward vector (typically Transform.forward)</param>
        /// <returns>True if the direction is pointing backward (within 45 degrees of opposite)</returns>
        public static bool IsBackward(this Vector3 direction, Vector3 forward)
        {
            float dotProduct = Vector3.Dot(forward.normalized, direction.normalized);
            return dotProduct <= Mathf.Cos(135f * Mathf.Deg2Rad);
        }

        /// <summary>
        /// Checks if the direction vector points frontally (either forward or backward).
        /// </summary>
        /// <param name="direction">The direction vector to check</param>
        /// <param name="forward">The reference forward vector (typically Transform.forward)</param>
        /// <returns>True if the direction is pointing forward or backward</returns>
        public static bool IsFrontal(this Vector3 direction, Vector3 forward)
        {
            return direction.IsForward(forward) || direction.IsBackward(forward);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Checks if the vector is approximately zero within a small tolerance.
        /// </summary>
        /// <param name="vector">The vector to check</param>
        /// <param name="tolerance">The tolerance for comparison (default: 0.0001f)</param>
        /// <returns>True if all components are approximately zero</returns>
        public static bool IsApproximatelyZero(this Vector3 vector, float tolerance = 0.0001f)
        {
            return vector.sqrMagnitude < tolerance * tolerance;
        }

        /// <summary>
        /// Returns a vector with each component rounded to the nearest integer.
        /// </summary>
        /// <param name="vector">The vector to round</param>
        /// <returns>A new vector with rounded components</returns>
        public static Vector3 Round(this Vector3 vector)
        {
            return new Vector3(
                Mathf.Round(vector.x),
                Mathf.Round(vector.y),
                Mathf.Round(vector.z)
            );
        }

        /// <summary>
        /// Returns a vector with each component floored to the nearest lower integer.
        /// </summary>
        /// <param name="vector">The vector to floor</param>
        /// <returns>A new vector with floored components</returns>
        public static Vector3 Floor(this Vector3 vector)
        {
            return new Vector3(
                Mathf.Floor(vector.x),
                Mathf.Floor(vector.y),
                Mathf.Floor(vector.z)
            );
        }

        /// <summary>
        /// Returns a vector with each component ceiled to the nearest higher integer.
        /// </summary>
        /// <param name="vector">The vector to ceil</param>
        /// <returns>A new vector with ceiled components</returns>
        public static Vector3 Ceil(this Vector3 vector)
        {
            return new Vector3(
                Mathf.Ceil(vector.x),
                Mathf.Ceil(vector.y),
                Mathf.Ceil(vector.z)
            );
        }

        #endregion
    }
}
