using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Extension methods for Rect providing utilities for position, size manipulation, and common operations.
    /// </summary>
    public static class RectExtensions
    {
        #region Position Getters

        /// <summary>
        /// Returns the center position of the Rect.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <returns>A Vector2 representing the center point</returns>
        public static Vector2 GetCenter(this Rect rect)
        {
            return new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f);
        }

        /// <summary>
        /// Returns the top-left corner position of the Rect.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <returns>A Vector2 representing the top-left corner</returns>
        public static Vector2 GetTopLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMin);
        }

        /// <summary>
        /// Returns the top-right corner position of the Rect.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <returns>A Vector2 representing the top-right corner</returns>
        public static Vector2 GetTopRight(this Rect rect)
        {
            return new Vector2(rect.xMax, rect.yMin);
        }

        /// <summary>
        /// Returns the bottom-left corner position of the Rect.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <returns>A Vector2 representing the bottom-left corner</returns>
        public static Vector2 GetBottomLeft(this Rect rect)
        {
            return new Vector2(rect.xMin, rect.yMax);
        }

        /// <summary>
        /// Returns the bottom-right corner position of the Rect.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <returns>A Vector2 representing the bottom-right corner</returns>
        public static Vector2 GetBottomRight(this Rect rect)
        {
            return new Vector2(rect.xMax, rect.yMax);
        }

        #endregion

        #region Position Setters

        /// <summary>
        /// Returns a new Rect with the X position set to the specified value.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="x">The new X position</param>
        /// <returns>A new Rect with the modified X position</returns>
        public static Rect SetX(this Rect rect, float x)
        {
            return new Rect(x, rect.y, rect.width, rect.height);
        }

        /// <summary>
        /// Returns a new Rect with the Y position set to the specified value.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="y">The new Y position</param>
        /// <returns>A new Rect with the modified Y position</returns>
        public static Rect SetY(this Rect rect, float y)
        {
            return new Rect(rect.x, y, rect.width, rect.height);
        }

        /// <summary>
        /// Returns a new Rect with the position set to the specified Vector2.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="position">The new position</param>
        /// <returns>A new Rect with the modified position</returns>
        public static Rect SetPosition(this Rect rect, Vector2 position)
        {
            return new Rect(position.x, position.y, rect.width, rect.height);
        }

        /// <summary>
        /// Returns a new Rect with the center position set to the specified Vector2.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="center">The new center position</param>
        /// <returns>A new Rect centered at the specified position</returns>
        public static Rect SetCenter(this Rect rect, Vector2 center)
        {
            return new Rect(
                center.x - rect.width * 0.5f,
                center.y - rect.height * 0.5f,
                rect.width,
                rect.height
            );
        }

        #endregion

        #region Position Modification

        /// <summary>
        /// Returns a new Rect with an offset added to the X position.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="x">The value to add to X</param>
        /// <returns>A new Rect with the modified X position</returns>
        public static Rect AddX(this Rect rect, float x)
        {
            return new Rect(rect.x + x, rect.y, rect.width, rect.height);
        }

        /// <summary>
        /// Returns a new Rect with an offset added to the Y position.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="y">The value to add to Y</param>
        /// <returns>A new Rect with the modified Y position</returns>
        public static Rect AddY(this Rect rect, float y)
        {
            return new Rect(rect.x, rect.y + y, rect.width, rect.height);
        }

        /// <summary>
        /// Returns a new Rect with a position offset applied.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="offset">The position offset to apply</param>
        /// <returns>A new Rect with the offset applied</returns>
        public static Rect AddPosition(this Rect rect, Vector2 offset)
        {
            return new Rect(rect.x + offset.x, rect.y + offset.y, rect.width, rect.height);
        }

        #endregion

        #region Size Setters

        /// <summary>
        /// Returns a new Rect with the width set to the specified value.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="width">The new width</param>
        /// <returns>A new Rect with the modified width</returns>
        public static Rect SetWidth(this Rect rect, float width)
        {
            return new Rect(rect.x, rect.y, width, rect.height);
        }

        /// <summary>
        /// Returns a new Rect with the height set to the specified value.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="height">The new height</param>
        /// <returns>A new Rect with the modified height</returns>
        public static Rect SetHeight(this Rect rect, float height)
        {
            return new Rect(rect.x, rect.y, rect.width, height);
        }

        /// <summary>
        /// Returns a new Rect with the size set to the specified Vector2.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="size">The new size</param>
        /// <returns>A new Rect with the modified size</returns>
        public static Rect SetSize(this Rect rect, Vector2 size)
        {
            return new Rect(rect.x, rect.y, size.x, size.y);
        }

        #endregion

        #region Size Modification

        /// <summary>
        /// Returns a new Rect with a value added to the width.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="width">The value to add to the width</param>
        /// <returns>A new Rect with the modified width</returns>
        public static Rect AddWidth(this Rect rect, float width)
        {
            return new Rect(rect.x, rect.y, rect.width + width, rect.height);
        }

        /// <summary>
        /// Returns a new Rect with a value added to the height.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="height">The value to add to the height</param>
        /// <returns>A new Rect with the modified height</returns>
        public static Rect AddHeight(this Rect rect, float height)
        {
            return new Rect(rect.x, rect.y, rect.width, rect.height + height);
        }

        /// <summary>
        /// Returns a new Rect with a size offset applied.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="sizeOffset">The size offset to apply (x = width delta, y = height delta)</param>
        /// <returns>A new Rect with the size offset applied</returns>
        public static Rect AddSize(this Rect rect, Vector2 sizeOffset)
        {
            return new Rect(rect.x, rect.y, rect.width + sizeOffset.x, rect.height + sizeOffset.y);
        }

        #endregion

        #region Scaling & Expansion

        /// <summary>
        /// Returns a new Rect scaled by the specified factor while maintaining the center position.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="scale">The scale factor</param>
        /// <returns>A new Rect scaled around its center</returns>
        /// <example>
        /// <code>
        /// Rect original = new Rect(0, 0, 100, 100);
        /// Rect scaled = original.Scale(0.5f); // Returns a 50x50 rect centered at the same position
        /// </code>
        /// </example>
        public static Rect Scale(this Rect rect, float scale)
        {
            Vector2 center = rect.GetCenter();
            float newWidth = rect.width * scale;
            float newHeight = rect.height * scale;

            return new Rect(
                center.x - newWidth * 0.5f,
                center.y - newHeight * 0.5f,
                newWidth,
                newHeight
            );
        }

        /// <summary>
        /// Returns a new Rect scaled by different factors for width and height while maintaining the center.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="scaleX">The width scale factor</param>
        /// <param name="scaleY">The height scale factor</param>
        /// <returns>A new Rect scaled around its center</returns>
        public static Rect Scale(this Rect rect, float scaleX, float scaleY)
        {
            Vector2 center = rect.GetCenter();
            float newWidth = rect.width * scaleX;
            float newHeight = rect.height * scaleY;

            return new Rect(
                center.x - newWidth * 0.5f,
                center.y - newHeight * 0.5f,
                newWidth,
                newHeight
            );
        }

        /// <summary>
        /// Returns a new Rect expanded by the specified amount on all sides.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="amount">The amount to expand on each side</param>
        /// <returns>A new Rect expanded by the specified amount</returns>
        /// <remarks>
        /// Positive values expand the rect, negative values shrink it.
        /// </remarks>
        public static Rect Expand(this Rect rect, float amount)
        {
            return new Rect(
                rect.x - amount,
                rect.y - amount,
                rect.width + amount * 2f,
                rect.height + amount * 2f
            );
        }

        /// <summary>
        /// Returns a new Rect expanded by different amounts horizontally and vertically.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="horizontal">The amount to expand horizontally (left and right)</param>
        /// <param name="vertical">The amount to expand vertically (top and bottom)</param>
        /// <returns>A new Rect expanded by the specified amounts</returns>
        public static Rect Expand(this Rect rect, float horizontal, float vertical)
        {
            return new Rect(
                rect.x - horizontal,
                rect.y - vertical,
                rect.width + horizontal * 2f,
                rect.height + vertical * 2f
            );
        }

        #endregion

        #region Containment & Overlap

        /// <summary>
        /// Checks if the Rect contains the specified point.
        /// </summary>
        /// <param name="rect">The Rect to check</param>
        /// <param name="point">The point to test</param>
        /// <returns>True if the point is inside the Rect</returns>
        public static bool ContainsPoint(this Rect rect, Vector2 point)
        {
            return rect.Contains(point);
        }

        /// <summary>
        /// Checks if this Rect completely contains another Rect.
        /// </summary>
        /// <param name="rect">The outer Rect</param>
        /// <param name="other">The inner Rect to test</param>
        /// <returns>True if the other Rect is completely inside this Rect</returns>
        public static bool ContainsRect(this Rect rect, Rect other)
        {
            return rect.xMin <= other.xMin &&
                   rect.xMax >= other.xMax &&
                   rect.yMin <= other.yMin &&
                   rect.yMax >= other.yMax;
        }

        /// <summary>
        /// Checks if this Rect overlaps with another Rect.
        /// </summary>
        /// <param name="rect">The first Rect</param>
        /// <param name="other">The second Rect</param>
        /// <returns>True if the Rects overlap</returns>
        public static bool Overlaps(this Rect rect, Rect other)
        {
            return rect.Overlaps(other);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Gets the area of the Rect (width * height).
        /// </summary>
        /// <param name="rect">The Rect</param>
        /// <returns>The area of the Rect</returns>
        public static float GetArea(this Rect rect)
        {
            return rect.width * rect.height;
        }

        /// <summary>
        /// Gets the perimeter of the Rect (2 * width + 2 * height).
        /// </summary>
        /// <param name="rect">The Rect</param>
        /// <returns>The perimeter of the Rect</returns>
        public static float GetPerimeter(this Rect rect)
        {
            return 2f * (rect.width + rect.height);
        }

        /// <summary>
        /// Gets the aspect ratio of the Rect (width / height).
        /// </summary>
        /// <param name="rect">The Rect</param>
        /// <returns>The aspect ratio, or 0 if height is zero</returns>
        public static float GetAspectRatio(this Rect rect)
        {
            if (Mathf.Approximately(rect.height, 0f))
                return 0f;

            return rect.width / rect.height;
        }

        /// <summary>
        /// Clamps a position to be within the Rect boundaries.
        /// </summary>
        /// <param name="rect">The Rect to clamp within</param>
        /// <param name="position">The position to clamp</param>
        /// <returns>The clamped position</returns>
        public static Vector2 ClampPosition(this Rect rect, Vector2 position)
        {
            return new Vector2(
                Mathf.Clamp(position.x, rect.xMin, rect.xMax),
                Mathf.Clamp(position.y, rect.yMin, rect.yMax)
            );
        }

        /// <summary>
        /// Converts the Rect to a string with formatted values.
        /// </summary>
        /// <param name="rect">The Rect</param>
        /// <param name="format">The format string for float values (e.g., "F2")</param>
        /// <returns>A formatted string representation of the Rect</returns>
        public static string ToString(this Rect rect, string format)
        {
            return $"(x:{rect.x.ToString(format)}, y:{rect.y.ToString(format)}, " +
                   $"width:{rect.width.ToString(format)}, height:{rect.height.ToString(format)})";
        }

        #endregion

        #region Legacy Support (Obsolete)

        /// <summary>
        /// Returns a new Rect with a value added to the width.
        /// </summary>
        /// <param name="rect">The source Rect</param>
        /// <param name="width">The value to add to the width</param>
        /// <returns>A new Rect with the modified width</returns>
        [System.Obsolete("Use AddWidth() instead. This method name had a typo.")]
        public static Rect AddWith(this Rect rect, float width)
        {
            return AddWidth(rect, width);
        }

        #endregion
    }
}
