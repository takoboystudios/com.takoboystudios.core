using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Extension methods for Color providing utilities for color manipulation and conversion.
    /// </summary>
    public static class ColorExtensions
    {
        #region Component Modification

        /// <summary>
        /// Returns a new Color with a modified alpha value.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="alpha">The new alpha value (0-1)</param>
        /// <returns>A new color with the specified alpha</returns>
        /// <example>
        /// <code>
        /// Color semiTransparent = Color.red.WithAlpha(0.5f);
        /// </code>
        /// </example>
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
        }

        /// <summary>
        /// Returns a new Color with a modified red value.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="red">The new red value (0-1)</param>
        /// <returns>A new color with the specified red component</returns>
        public static Color WithRed(this Color color, float red)
        {
            return new Color(Mathf.Clamp01(red), color.g, color.b, color.a);
        }

        /// <summary>
        /// Returns a new Color with a modified green value.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="green">The new green value (0-1)</param>
        /// <returns>A new color with the specified green component</returns>
        public static Color WithGreen(this Color color, float green)
        {
            return new Color(color.r, Mathf.Clamp01(green), color.b, color.a);
        }

        /// <summary>
        /// Returns a new Color with a modified blue value.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="blue">The new blue value (0-1)</param>
        /// <returns>A new color with the specified blue component</returns>
        public static Color WithBlue(this Color color, float blue)
        {
            return new Color(color.r, color.g, Mathf.Clamp01(blue), color.a);
        }

        #endregion

        #region Color Transformations

        /// <summary>
        /// Returns a new Color with inverted RGB values (1 - value).
        /// </summary>
        /// <param name="color">The source color</param>
        /// <returns>A new color with inverted RGB values (alpha unchanged)</returns>
        /// <remarks>
        /// This produces the complementary color by inverting each RGB component.
        /// The alpha channel is preserved.
        /// </remarks>
        /// <example>
        /// <code>
        /// Color black = Color.white.Invert(); // Returns black
        /// Color white = Color.black.Invert(); // Returns white
        /// </code>
        /// </example>
        public static Color Invert(this Color color)
        {
            return new Color(1.0f - color.r, 1.0f - color.g, 1.0f - color.b, color.a);
        }

        /// <summary>
        /// Returns the grayscale version of the color using Unity's built-in grayscale calculation.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <returns>A grayscale color (alpha is preserved)</returns>
        /// <remarks>
        /// Unity's grayscale calculation uses perceptual luminance weights:
        /// 0.299*R + 0.587*G + 0.114*B
        /// </remarks>
        public static Color ToGrayscale(this Color color)
        {
            float gray = color.grayscale;
            return new Color(gray, gray, gray, color.a);
        }

        /// <summary>
        /// Multiplies the RGB values by a brightness factor.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="brightness">The brightness multiplier (0 = black, 1 = unchanged, >1 = brighter)</param>
        /// <returns>A new color with adjusted brightness</returns>
        public static Color AdjustBrightness(this Color color, float brightness)
        {
            return new Color(
                Mathf.Clamp01(color.r * brightness),
                Mathf.Clamp01(color.g * brightness),
                Mathf.Clamp01(color.b * brightness),
                color.a
            );
        }

        /// <summary>
        /// Adjusts the saturation of the color.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="saturation">The saturation level (0 = grayscale, 1 = unchanged, >1 = more saturated)</param>
        /// <returns>A new color with adjusted saturation</returns>
        public static Color AdjustSaturation(this Color color, float saturation)
        {
            float gray = color.grayscale;
            
            return new Color(
                Mathf.Clamp01(Mathf.Lerp(gray, color.r, saturation)),
                Mathf.Clamp01(Mathf.Lerp(gray, color.g, saturation)),
                Mathf.Clamp01(Mathf.Lerp(gray, color.b, saturation)),
                color.a
            );
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Converts a hex color string to a Color.
        /// </summary>
        /// <param name="hex">The hex color string (e.g., "#FF0000", "FF0000", "#F00")</param>
        /// <param name="alpha">The alpha value to apply (0-1)</param>
        /// <returns>The parsed color, or white if parsing fails</returns>
        /// <remarks>
        /// Supports formats: #RRGGBB, RRGGBB, #RGB, RGB, #RRGGBBAA, RRGGBBAA
        /// If parsing fails, returns white with the specified alpha.
        /// </remarks>
        /// <example>
        /// <code>
        /// Color red = "#FF0000".ToColor();
        /// Color semiTransparentBlue = "0000FF".ToColor(0.5f);
        /// </code>
        /// </example>
        public static Color ToColor(this string hex, float alpha = 1f)
        {
            if (string.IsNullOrEmpty(hex))
            {
                Debug.LogWarning("[ColorExtensions] Cannot convert null or empty string to Color. Returning white.");
                return new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            }

            // Try to parse the hex string
            if (ColorUtility.TryParseHtmlString(hex, out Color newColor))
            {
                newColor.a = Mathf.Clamp01(alpha);
                return newColor;
            }

            // If parsing failed, try adding # prefix if not present
            if (!hex.StartsWith("#"))
            {
                if (ColorUtility.TryParseHtmlString("#" + hex, out newColor))
                {
                    newColor.a = Mathf.Clamp01(alpha);
                    return newColor;
                }
            }

            Debug.LogWarning($"[ColorExtensions] Failed to parse color from hex string: '{hex}'. Returning white.");
            return new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
        }

        /// <summary>
        /// Converts the color to a hex string.
        /// </summary>
        /// <param name="color">The color to convert</param>
        /// <param name="includeAlpha">If true, includes alpha in the output (RRGGBBAA format)</param>
        /// <returns>A hex string representation of the color (e.g., "#FF0000" or "#FF0000FF")</returns>
        public static string ToHex(this Color color, bool includeAlpha = false)
        {
            if (includeAlpha)
            {
                return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
            }
            else
            {
                return $"#{ColorUtility.ToHtmlStringRGB(color)}";
            }
        }

        #endregion

        #region Blending & Interpolation

        /// <summary>
        /// Blends two colors using linear interpolation.
        /// </summary>
        /// <param name="color">The first color</param>
        /// <param name="target">The second color</param>
        /// <param name="t">The interpolation value (0-1, where 0 = first color, 1 = second color)</param>
        /// <returns>The blended color</returns>
        public static Color Blend(this Color color, Color target, float t)
        {
            return Color.Lerp(color, target, Mathf.Clamp01(t));
        }

        /// <summary>
        /// Multiplies two colors component-wise.
        /// </summary>
        /// <param name="color">The first color</param>
        /// <param name="other">The second color</param>
        /// <returns>A new color with each component multiplied</returns>
        /// <remarks>
        /// This is useful for tinting operations.
        /// </remarks>
        public static Color Multiply(this Color color, Color other)
        {
            return new Color(
                color.r * other.r,
                color.g * other.g,
                color.b * other.b,
                color.a * other.a
            );
        }

        /// <summary>
        /// Adds two colors component-wise (clamped to valid range).
        /// </summary>
        /// <param name="color">The first color</param>
        /// <param name="other">The second color</param>
        /// <returns>A new color with each component added and clamped</returns>
        public static Color Add(this Color color, Color other)
        {
            return new Color(
                Mathf.Clamp01(color.r + other.r),
                Mathf.Clamp01(color.g + other.g),
                Mathf.Clamp01(color.b + other.b),
                Mathf.Clamp01(color.a + other.a)
            );
        }

        #endregion

        #region Comparison & Utility

        /// <summary>
        /// Checks if two colors are approximately equal within a small tolerance.
        /// </summary>
        /// <param name="color">The first color</param>
        /// <param name="other">The second color</param>
        /// <param name="tolerance">The tolerance for comparison (default: 0.01f)</param>
        /// <returns>True if all components are within tolerance</returns>
        public static bool IsApproximately(this Color color, Color other, float tolerance = 0.01f)
        {
            return Mathf.Abs(color.r - other.r) < tolerance &&
                   Mathf.Abs(color.g - other.g) < tolerance &&
                   Mathf.Abs(color.b - other.b) < tolerance &&
                   Mathf.Abs(color.a - other.a) < tolerance;
        }

        /// <summary>
        /// Gets the perceived brightness of the color (0-1).
        /// </summary>
        /// <param name="color">The color to evaluate</param>
        /// <returns>The perceived brightness value</returns>
        /// <remarks>
        /// This is the same as Unity's built-in grayscale property.
        /// </remarks>
        public static float GetBrightness(this Color color)
        {
            return color.grayscale;
        }

        /// <summary>
        /// Checks if the color is considered "dark" based on its perceived brightness.
        /// </summary>
        /// <param name="color">The color to evaluate</param>
        /// <param name="threshold">The brightness threshold (default: 0.5)</param>
        /// <returns>True if the color's brightness is below the threshold</returns>
        public static bool IsDark(this Color color, float threshold = 0.5f)
        {
            return color.grayscale < threshold;
        }

        /// <summary>
        /// Checks if the color is considered "light" based on its perceived brightness.
        /// </summary>
        /// <param name="color">The color to evaluate</param>
        /// <param name="threshold">The brightness threshold (default: 0.5)</param>
        /// <returns>True if the color's brightness is above the threshold</returns>
        public static bool IsLight(this Color color, float threshold = 0.5f)
        {
            return color.grayscale >= threshold;
        }

        /// <summary>
        /// Returns an appropriate contrasting color (black or white) based on the input color's brightness.
        /// </summary>
        /// <param name="color">The background color</param>
        /// <returns>Either Color.black or Color.white for optimal contrast</returns>
        /// <remarks>
        /// Useful for determining text color on a background to ensure readability.
        /// </remarks>
        public static Color GetContrastingColor(this Color color)
        {
            return color.IsDark() ? Color.white : Color.black;
        }

        #endregion

        #region Legacy Support (Obsolete)

        /// <summary>
        /// Returns a new Color with a modified alpha value.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="alpha">The new alpha value</param>
        /// <returns>A new color with the specified alpha</returns>
        [System.Obsolete("Use WithAlpha() instead for consistent naming.")]
        public static Color Alpha(this Color color, float alpha)
        {
            return WithAlpha(color, alpha);
        }

        /// <summary>
        /// Returns a new Color with a modified red value.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="red">The new red value</param>
        /// <returns>A new color with the specified red component</returns>
        [System.Obsolete("Use WithRed() instead for consistent naming.")]
        public static Color Red(this Color color, float red)
        {
            return WithRed(color, red);
        }

        /// <summary>
        /// Returns a new Color with a modified green value.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="green">The new green value</param>
        /// <returns>A new color with the specified green component</returns>
        [System.Obsolete("Use WithGreen() instead for consistent naming.")]
        public static Color Green(this Color color, float green)
        {
            return WithGreen(color, green);
        }

        /// <summary>
        /// Returns a new Color with a modified blue value.
        /// </summary>
        /// <param name="color">The source color</param>
        /// <param name="blue">The new blue value</param>
        /// <returns>A new color with the specified blue component</returns>
        [System.Obsolete("Use WithBlue() instead for consistent naming.")]
        public static Color Blue(this Color color, float blue)
        {
            return WithBlue(color, blue);
        }

        #endregion
    }
}
