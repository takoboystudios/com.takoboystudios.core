using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Extension methods for Vector2 providing utilities for 2D vector operations, rotations, and conversions.
    /// </summary>
    public static class Vector2Extensions
    {
        #region Component Setters

        /// <summary>
        /// Returns a new Vector2 with the X component set to the specified value.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="x">The new X value</param>
        /// <returns>A new Vector2 with the modified X component</returns>
        public static Vector2 SetX(this Vector2 vector, float x)
        {
            return new Vector2(x, vector.y);
        }

        /// <summary>
        /// Returns a new Vector2 with the Y component set to the specified value.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="y">The new Y value</param>
        /// <returns>A new Vector2 with the modified Y component</returns>
        public static Vector2 SetY(this Vector2 vector, float y)
        {
            return new Vector2(vector.x, y);
        }

        /// <summary>
        /// Returns a new Vector2 with both components set at once.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="x">The new X value (null to keep current)</param>
        /// <param name="y">The new Y value (null to keep current)</param>
        /// <returns>A new Vector2 with the specified components modified</returns>
        public static Vector2 Set(this Vector2 vector, float? x = null, float? y = null)
        {
            return new Vector2(
                x ?? vector.x,
                y ?? vector.y
            );
        }

        #endregion

        #region Component Addition

        /// <summary>
        /// Returns a new Vector2 with a value added to the X component.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="x">The value to add to X</param>
        /// <returns>A new Vector2 with the modified X component</returns>
        public static Vector2 AddX(this Vector2 vector, float x)
        {
            return new Vector2(vector.x + x, vector.y);
        }

        /// <summary>
        /// Returns a new Vector2 with a value added to the Y component.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <param name="y">The value to add to Y</param>
        /// <returns>A new Vector2 with the modified Y component</returns>
        public static Vector2 AddY(this Vector2 vector, float y)
        {
            return new Vector2(vector.x, vector.y + y);
        }

        #endregion

        #region Conversions

        /// <summary>
        /// Converts a Vector2 to Vector3 with the specified Z value.
        /// </summary>
        /// <param name="vector">The source Vector2</param>
        /// <param name="z">The Z component for the Vector3 (default: 0)</param>
        /// <returns>A Vector3 with (x, y, z)</returns>
        public static Vector3 ToVector3(this Vector2 vector, float z = 0f)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        /// <summary>
        /// Converts a Vector2 to Vector3 with the Vector2 representing X and Z (Y is the vertical axis).
        /// </summary>
        /// <param name="vector">The source Vector2</param>
        /// <param name="y">The Y component for the Vector3 (default: 0)</param>
        /// <returns>A Vector3 with (x, y, z) where x and z come from the Vector2</returns>
        /// <remarks>
        /// Useful for converting 2D positions to 3D horizontal plane positions.
        /// </remarks>
        public static Vector3 ToVector3XZ(this Vector2 vector, float y = 0f)
        {
            return new Vector3(vector.x, y, vector.y);
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
        /// More efficient than Distance for comparisons as it avoids the square root calculation.
        /// </remarks>
        public static float SqrDistance(this Vector2 vector, Vector2 other)
        {
            return (vector - other).sqrMagnitude;
        }

        /// <summary>
        /// Calculates the distance between two vectors.
        /// </summary>
        /// <param name="vector">The first vector</param>
        /// <param name="other">The second vector</param>
        /// <returns>The distance between the vectors</returns>
        public static float Distance(this Vector2 vector, Vector2 other)
        {
            return Vector2.Distance(vector, other);
        }

        #endregion

        #region Rotation

        /// <summary>
        /// Rotates the vector by the specified angle in degrees.
        /// </summary>
        /// <param name="vector">The vector to rotate</param>
        /// <param name="degrees">The angle in degrees to rotate (positive = counter-clockwise)</param>
        /// <returns>The rotated vector</returns>
        /// <example>
        /// <code>
        /// Vector2 right = Vector2.right;
        /// Vector2 up = right.Rotate(90f); // Rotates 90° counter-clockwise to get (0, 1)
        /// </code>
        /// </example>
        public static Vector2 Rotate(this Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);

            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }

        /// <summary>
        /// Rotates the vector by the specified angle in radians.
        /// </summary>
        /// <param name="vector">The vector to rotate</param>
        /// <param name="radians">The angle in radians to rotate (positive = counter-clockwise)</param>
        /// <returns>The rotated vector</returns>
        public static Vector2 RotateRadians(this Vector2 vector, float radians)
        {
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);

            return new Vector2(
                vector.x * cos - vector.y * sin,
                vector.x * sin + vector.y * cos
            );
        }

        /// <summary>
        /// Rotates the vector to face the specified direction.
        /// </summary>
        /// <param name="vector">The vector to rotate</param>
        /// <param name="direction">The target direction</param>
        /// <returns>The rotated vector maintaining its magnitude but facing the new direction</returns>
        /// <remarks>
        /// The resulting vector will have the same magnitude as the input vector but will
        /// point in the direction of the target direction vector.
        /// </remarks>
        /// <example>
        /// <code>
        /// Vector2 velocity = new Vector2(5, 0); // Moving right at speed 5
        /// Vector2 targetDir = new Vector2(0, 1); // Want to face up
        /// Vector2 newVelocity = velocity.FaceDirection(targetDir); // Results in (0, 5)
        /// </code>
        /// </example>
        public static Vector2 FaceDirection(this Vector2 vector, Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                Debug.LogWarning("[Vector2Extensions] Cannot face zero direction. Returning original vector.");
                return vector;
            }

            float magnitude = vector.magnitude;
            return direction.normalized * magnitude;
        }

        /// <summary>
        /// Rotates the vector to point towards a target position.
        /// </summary>
        /// <param name="vector">The vector to rotate (treated as a position)</param>
        /// <param name="target">The target position to face</param>
        /// <param name="magnitude">The magnitude of the resulting vector (default: 1)</param>
        /// <returns>A vector pointing from the source to the target with the specified magnitude</returns>
        public static Vector2 FacePosition(this Vector2 vector, Vector2 target, float magnitude = 1f)
        {
            Vector2 direction = (target - vector).normalized;
            return direction * magnitude;
        }

        /// <summary>
        /// Gets the angle of the vector in degrees.
        /// </summary>
        /// <param name="vector">The vector</param>
        /// <returns>The angle in degrees (0-360), where 0° is right (1, 0)</returns>
        /// <example>
        /// <code>
        /// Vector2 up = Vector2.up;
        /// float angle = up.GetAngle(); // Returns 90
        /// </code>
        /// </example>
        public static float GetAngle(this Vector2 vector)
        {
            float angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f;
            return angle;
        }

        /// <summary>
        /// Gets the signed angle of the vector in degrees.
        /// </summary>
        /// <param name="vector">The vector</param>
        /// <returns>The angle in degrees (-180 to 180), where 0° is right (1, 0)</returns>
        public static float GetAngleSigned(this Vector2 vector)
        {
            return Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Gets the angle between two vectors in degrees.
        /// </summary>
        /// <param name="from">The first vector</param>
        /// <param name="to">The second vector</param>
        /// <returns>The angle between the vectors (0-180)</returns>
        public static float AngleTo(this Vector2 from, Vector2 to)
        {
            return Vector2.Angle(from, to);
        }

        /// <summary>
        /// Gets the signed angle from one vector to another in degrees.
        /// </summary>
        /// <param name="from">The first vector</param>
        /// <param name="to">The second vector</param>
        /// <returns>The signed angle (-180 to 180), positive = counter-clockwise</returns>
        public static float SignedAngleTo(this Vector2 from, Vector2 to)
        {
            return Vector2.SignedAngle(from, to);
        }

        #endregion

        #region Perpendicular & Normal

        /// <summary>
        /// Returns a vector perpendicular to this vector (rotated 90° counter-clockwise).
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <returns>A perpendicular vector</returns>
        /// <remarks>
        /// For a vector (x, y), returns (-y, x).
        /// </remarks>
        public static Vector2 Perpendicular(this Vector2 vector)
        {
            return new Vector2(-vector.y, vector.x);
        }

        /// <summary>
        /// Returns a vector perpendicular to this vector (rotated 90° clockwise).
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <returns>A perpendicular vector</returns>
        /// <remarks>
        /// For a vector (x, y), returns (y, -x).
        /// </remarks>
        public static Vector2 PerpendicularClockwise(this Vector2 vector)
        {
            return new Vector2(vector.y, -vector.x);
        }

        #endregion

        #region Mathematical Operations

        /// <summary>
        /// Returns the absolute value of each component.
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <returns>A new vector with absolute values</returns>
        public static Vector2 Abs(this Vector2 vector)
        {
            return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        }

        /// <summary>
        /// Inverts the vector (multiplies by -1).
        /// </summary>
        /// <param name="vector">The source vector</param>
        /// <returns>The inverted vector</returns>
        public static Vector2 Invert(this Vector2 vector)
        {
            return -vector;
        }

        /// <summary>
        /// Clamps each component between corresponding min and max values.
        /// </summary>
        /// <param name="vector">The vector to clamp</param>
        /// <param name="min">The minimum values for each component</param>
        /// <param name="max">The maximum values for each component</param>
        /// <returns>A new vector with clamped components</returns>
        public static Vector2 Clamp(this Vector2 vector, Vector2 min, Vector2 max)
        {
            return new Vector2(
                Mathf.Clamp(vector.x, min.x, max.x),
                Mathf.Clamp(vector.y, min.y, max.y)
            );
        }

        /// <summary>
        /// Clamps the magnitude of the vector to the specified maximum length.
        /// </summary>
        /// <param name="vector">The vector to clamp</param>
        /// <param name="maxLength">The maximum allowed magnitude</param>
        /// <returns>A vector with clamped magnitude</returns>
        public static Vector2 ClampMagnitude(this Vector2 vector, float maxLength)
        {
            return Vector2.ClampMagnitude(vector, maxLength);
        }

        /// <summary>
        /// Returns a vector with each component rounded to the nearest integer.
        /// </summary>
        /// <param name="vector">The vector to round</param>
        /// <returns>A new vector with rounded components</returns>
        public static Vector2 Round(this Vector2 vector)
        {
            return new Vector2(Mathf.Round(vector.x), Mathf.Round(vector.y));
        }

        /// <summary>
        /// Returns a vector with each component floored to the nearest lower integer.
        /// </summary>
        /// <param name="vector">The vector to floor</param>
        /// <returns>A new vector with floored components</returns>
        public static Vector2 Floor(this Vector2 vector)
        {
            return new Vector2(Mathf.Floor(vector.x), Mathf.Floor(vector.y));
        }

        /// <summary>
        /// Returns a vector with each component ceiled to the nearest higher integer.
        /// </summary>
        /// <param name="vector">The vector to ceil</param>
        /// <returns>A new vector with ceiled components</returns>
        public static Vector2 Ceil(this Vector2 vector)
        {
            return new Vector2(Mathf.Ceil(vector.x), Mathf.Ceil(vector.y));
        }

        #endregion

        #region Direction Checking

        /// <summary>
        /// Checks if the vector is pointing to the right (within 45 degrees of (1, 0)).
        /// </summary>
        /// <param name="vector">The vector to check</param>
        /// <returns>True if pointing right</returns>
        public static bool IsPointingRight(this Vector2 vector)
        {
            return vector.normalized.x > 0.707f; // cos(45°)
        }

        /// <summary>
        /// Checks if the vector is pointing to the left (within 45 degrees of (-1, 0)).
        /// </summary>
        /// <param name="vector">The vector to check</param>
        /// <returns>True if pointing left</returns>
        public static bool IsPointingLeft(this Vector2 vector)
        {
            return vector.normalized.x < -0.707f; // cos(135°)
        }

        /// <summary>
        /// Checks if the vector is pointing up (within 45 degrees of (0, 1)).
        /// </summary>
        /// <param name="vector">The vector to check</param>
        /// <returns>True if pointing up</returns>
        public static bool IsPointingUp(this Vector2 vector)
        {
            return vector.normalized.y > 0.707f; // cos(45°)
        }

        /// <summary>
        /// Checks if the vector is pointing down (within 45 degrees of (0, -1)).
        /// </summary>
        /// <param name="vector">The vector to check</param>
        /// <returns>True if pointing down</returns>
        public static bool IsPointingDown(this Vector2 vector)
        {
            return vector.normalized.y < -0.707f; // cos(135°)
        }

        #endregion

        #region Utility

        /// <summary>
        /// Checks if the vector is approximately zero within a small tolerance.
        /// </summary>
        /// <param name="vector">The vector to check</param>
        /// <param name="tolerance">The tolerance for comparison (default: 0.0001f)</param>
        /// <returns>True if both components are approximately zero</returns>
        public static bool IsApproximatelyZero(this Vector2 vector, float tolerance = 0.0001f)
        {
            return vector.sqrMagnitude < tolerance * tolerance;
        }

        /// <summary>
        /// Checks if two vectors are approximately equal within a small tolerance.
        /// </summary>
        /// <param name="vector">The first vector</param>
        /// <param name="other">The second vector</param>
        /// <param name="tolerance">The tolerance for comparison (default: 0.0001f)</param>
        /// <returns>True if the vectors are approximately equal</returns>
        public static bool IsApproximately(this Vector2 vector, Vector2 other, float tolerance = 0.0001f)
        {
            return (vector - other).sqrMagnitude < tolerance * tolerance;
        }

        /// <summary>
        /// Multiplies each component by the corresponding component of another vector.
        /// </summary>
        /// <param name="vector">The first vector</param>
        /// <param name="other">The second vector</param>
        /// <returns>A new vector with component-wise multiplication</returns>
        public static Vector2 Multiply(this Vector2 vector, Vector2 other)
        {
            return new Vector2(vector.x * other.x, vector.y * other.y);
        }

        /// <summary>
        /// Divides each component by the corresponding component of another vector.
        /// </summary>
        /// <param name="vector">The numerator vector</param>
        /// <param name="other">The denominator vector</param>
        /// <returns>A new vector with component-wise division</returns>
        /// <remarks>
        /// Be careful of division by zero. Components with zero denominators will result in Infinity or NaN.
        /// </remarks>
        public static Vector2 Divide(this Vector2 vector, Vector2 other)
        {
            return new Vector2(vector.x / other.x, vector.y / other.y);
        }

        /// <summary>
        /// Gets the dominant axis of the vector (the component with the larger absolute value).
        /// </summary>
        /// <param name="vector">The vector</param>
        /// <returns>0 for X-dominant, 1 for Y-dominant</returns>
        public static int GetDominantAxis(this Vector2 vector)
        {
            return Mathf.Abs(vector.x) > Mathf.Abs(vector.y) ? 0 : 1;
        }

        /// <summary>
        /// Snaps the vector to the nearest axis (returns unit vector on dominant axis).
        /// </summary>
        /// <param name="vector">The vector to snap</param>
        /// <returns>Either Vector2.right, Vector2.left, Vector2.up, or Vector2.down</returns>
        public static Vector2 SnapToAxis(this Vector2 vector)
        {
            if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
            {
                return vector.x > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                return vector.y > 0 ? Vector2.up : Vector2.down;
            }
        }

        /// <summary>
        /// Projects the vector onto another vector.
        /// </summary>
        /// <param name="vector">The vector to project</param>
        /// <param name="onNormal">The vector to project onto (will be normalized)</param>
        /// <returns>The projected vector</returns>
        public static Vector2 ProjectOnto(this Vector2 vector, Vector2 onNormal)
        {
            float sqrMag = onNormal.sqrMagnitude;
            if (sqrMag < 0.0001f)
                return Vector2.zero;

            float dot = Vector2.Dot(vector, onNormal);
            return onNormal * (dot / sqrMag);
        }

        /// <summary>
        /// Reflects the vector off a surface defined by a normal.
        /// </summary>
        /// <param name="vector">The incoming vector</param>
        /// <param name="normal">The surface normal (should be normalized)</param>
        /// <returns>The reflected vector</returns>
        public static Vector2 Reflect(this Vector2 vector, Vector2 normal)
        {
            return Vector2.Reflect(vector, normal);
        }

        #endregion

        #region Legacy Support (Obsolete)

        /// <summary>
        /// Rotates the vector to the specified angle in radians.
        /// </summary>
        /// <param name="vector">The vector to rotate</param>
        /// <param name="radians">The target angle in radians</param>
        /// <returns>The rotated vector</returns>
        [System.Obsolete("Use RotateRadians() for clarity, or Rotate() with degrees.")]
        public static Vector2 RotateToAngle(this Vector2 vector, float radians)
        {
            return vector.RotateRadians(radians);
        }

        #endregion
    }
}
