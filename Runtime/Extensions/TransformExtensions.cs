using System.Collections.Generic;
using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Extension methods for Transform and GameObject providing utilities for hierarchy manipulation
    /// and component access.
    /// </summary>
    public static class TransformExtensions
    {
        #region Hierarchy Access

        /// <summary>
        /// Returns only the direct children of the Transform, excluding grandchildren and deeper descendants.
        /// </summary>
        /// <param name="transform">The parent transform</param>
        /// <returns>An array of direct child transforms</returns>
        /// <remarks>
        /// This is useful when you want to iterate only through immediate children without
        /// recursing through the entire hierarchy, which GetComponentsInChildren does by default.
        /// </remarks>
        /// <example>
        /// <code>
        /// Transform[] directChildren = transform.GetDirectChildren();
        /// foreach (Transform child in directChildren)
        /// {
        ///     Debug.Log($"Direct child: {child.name}");
        /// }
        /// </code>
        /// </example>
        public static Transform[] GetDirectChildren(this Transform transform)
        {
            if (transform == null)
            {
                Debug.LogWarning("[TransformExtensions] GetDirectChildren called on null transform");
                return new Transform[0];
            }

            int childCount = transform.childCount;
            Transform[] children = new Transform[childCount];

            for (int i = 0; i < childCount; i++)
            {
                children[i] = transform.GetChild(i);
            }

            return children;
        }

        /// <summary>
        /// Returns components of type T from only the direct children, excluding grandchildren and deeper descendants.
        /// </summary>
        /// <typeparam name="T">The component type to search for</typeparam>
        /// <param name="transform">The parent transform</param>
        /// <param name="includeInactive">Whether to include components on inactive GameObjects</param>
        /// <returns>An array of components found on direct children</returns>
        /// <remarks>
        /// Unlike GetComponentsInChildren, this method does not recurse through the entire hierarchy.
        /// Only checks the immediate children of the transform.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Get all SpriteRenderer components on direct children only
        /// SpriteRenderer[] childRenderers = transform.GetComponentsInDirectChildren&lt;SpriteRenderer&gt;();
        /// </code>
        /// </example>
        public static T[] GetComponentsInDirectChildren<T>(this Transform transform, bool includeInactive = false) 
            where T : Component
        {
            if (transform == null)
            {
                Debug.LogWarning("[TransformExtensions] GetComponentsInDirectChildren called on null transform");
                return new T[0];
            }

            List<T> components = new List<T>();
            int childCount = transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                
                // Skip inactive children if specified
                if (!includeInactive && !child.gameObject.activeInHierarchy)
                    continue;

                T component = child.GetComponent<T>();
                if (component != null)
                {
                    components.Add(component);
                }
            }

            return components.ToArray();
        }

        /// <summary>
        /// Returns components of type T from only the direct children of the GameObject.
        /// </summary>
        /// <typeparam name="T">The component type to search for</typeparam>
        /// <param name="gameObject">The parent GameObject</param>
        /// <param name="includeInactive">Whether to include components on inactive GameObjects</param>
        /// <returns>An array of components found on direct children</returns>
        public static T[] GetComponentsInDirectChildren<T>(this GameObject gameObject, bool includeInactive = false) 
            where T : Component
        {
            if (gameObject == null)
            {
                Debug.LogWarning("[TransformExtensions] GetComponentsInDirectChildren called on null GameObject");
                return new T[0];
            }

            return gameObject.transform.GetComponentsInDirectChildren<T>(includeInactive);
        }

        #endregion

        #region Hierarchy Manipulation

        /// <summary>
        /// Destroys all direct children of the transform.
        /// </summary>
        /// <param name="transform">The parent transform</param>
        /// <param name="immediate">If true, uses DestroyImmediate instead of Destroy</param>
        /// <remarks>
        /// Use immediate = true only in the Editor when not in Play mode.
        /// In Play mode, always use immediate = false to avoid issues.
        /// </remarks>
        public static void DestroyChildren(this Transform transform, bool immediate = false)
        {
            if (transform == null)
            {
                Debug.LogWarning("[TransformExtensions] DestroyChildren called on null transform");
                return;
            }

            // Get children before destroying to avoid modifying collection during iteration
            Transform[] children = transform.GetDirectChildren();

            foreach (Transform child in children)
            {
                if (child != null)
                {
                    if (immediate)
                        Object.DestroyImmediate(child.gameObject);
                    else
                        Object.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Destroys all direct children of the GameObject.
        /// </summary>
        /// <param name="gameObject">The parent GameObject</param>
        /// <param name="immediate">If true, uses DestroyImmediate instead of Destroy</param>
        public static void DestroyChildren(this GameObject gameObject, bool immediate = false)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("[TransformExtensions] DestroyChildren called on null GameObject");
                return;
            }

            gameObject.transform.DestroyChildren(immediate);
        }

        /// <summary>
        /// Sets the parent of the transform and optionally resets its local position, rotation, and scale.
        /// </summary>
        /// <param name="transform">The transform to reparent</param>
        /// <param name="parent">The new parent (can be null)</param>
        /// <param name="resetTransform">If true, resets local position to zero, rotation to identity, and scale to one</param>
        public static void SetParentAndReset(this Transform transform, Transform parent, bool resetTransform = true)
        {
            if (transform == null)
            {
                Debug.LogWarning("[TransformExtensions] SetParentAndReset called on null transform");
                return;
            }

            transform.SetParent(parent, !resetTransform);

            if (resetTransform)
            {
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;
            }
        }

        #endregion

        #region Position Helpers

        /// <summary>
        /// Resets the transform's local position, rotation, and scale to default values.
        /// </summary>
        /// <param name="transform">The transform to reset</param>
        public static void ResetLocal(this Transform transform)
        {
            if (transform == null)
            {
                Debug.LogWarning("[TransformExtensions] ResetLocal called on null transform");
                return;
            }

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Sets the world position while preserving the local rotation and scale.
        /// </summary>
        /// <param name="transform">The transform to modify</param>
        /// <param name="x">The world X position (null to keep current)</param>
        /// <param name="y">The world Y position (null to keep current)</param>
        /// <param name="z">The world Z position (null to keep current)</param>
        public static void SetWorldPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            if (transform == null)
            {
                Debug.LogWarning("[TransformExtensions] SetWorldPosition called on null transform");
                return;
            }

            Vector3 position = transform.position;
            transform.position = new Vector3(
                x ?? position.x,
                y ?? position.y,
                z ?? position.z
            );
        }

        /// <summary>
        /// Sets the local position.
        /// </summary>
        /// <param name="transform">The transform to modify</param>
        /// <param name="x">The local X position (null to keep current)</param>
        /// <param name="y">The local Y position (null to keep current)</param>
        /// <param name="z">The local Z position (null to keep current)</param>
        public static void SetLocalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            if (transform == null)
            {
                Debug.LogWarning("[TransformExtensions] SetLocalPosition called on null transform");
                return;
            }

            Vector3 position = transform.localPosition;
            transform.localPosition = new Vector3(
                x ?? position.x,
                y ?? position.y,
                z ?? position.z
            );
        }

        #endregion

        #region Distance & Direction

        /// <summary>
        /// Calculates the distance between two transforms.
        /// </summary>
        /// <param name="transform">The first transform</param>
        /// <param name="other">The second transform</param>
        /// <returns>The distance between the transforms</returns>
        public static float Distance(this Transform transform, Transform other)
        {
            if (transform == null || other == null)
            {
                Debug.LogWarning("[TransformExtensions] Distance called with null transform(s)");
                return float.MaxValue;
            }

            return Vector3.Distance(transform.position, other.position);
        }

        /// <summary>
        /// Calculates the squared distance between two transforms.
        /// </summary>
        /// <param name="transform">The first transform</param>
        /// <param name="other">The second transform</param>
        /// <returns>The squared distance between the transforms</returns>
        /// <remarks>
        /// More efficient than Distance when you only need to compare distances.
        /// </remarks>
        public static float SqrDistance(this Transform transform, Transform other)
        {
            if (transform == null || other == null)
            {
                Debug.LogWarning("[TransformExtensions] SqrDistance called with null transform(s)");
                return float.MaxValue;
            }

            return (transform.position - other.position).sqrMagnitude;
        }

        /// <summary>
        /// Gets the direction vector from this transform to another transform.
        /// </summary>
        /// <param name="transform">The source transform</param>
        /// <param name="target">The target transform</param>
        /// <param name="normalized">If true, returns a normalized direction vector</param>
        /// <returns>The direction vector from this transform to the target</returns>
        public static Vector3 DirectionTo(this Transform transform, Transform target, bool normalized = true)
        {
            if (transform == null || target == null)
            {
                Debug.LogWarning("[TransformExtensions] DirectionTo called with null transform(s)");
                return Vector3.zero;
            }

            Vector3 direction = target.position - transform.position;
            return normalized ? direction.normalized : direction;
        }

        /// <summary>
        /// Gets the direction vector from this transform to a world position.
        /// </summary>
        /// <param name="transform">The source transform</param>
        /// <param name="worldPosition">The target world position</param>
        /// <param name="normalized">If true, returns a normalized direction vector</param>
        /// <returns>The direction vector from this transform to the position</returns>
        public static Vector3 DirectionTo(this Transform transform, Vector3 worldPosition, bool normalized = true)
        {
            if (transform == null)
            {
                Debug.LogWarning("[TransformExtensions] DirectionTo called with null transform");
                return Vector3.zero;
            }

            Vector3 direction = worldPosition - transform.position;
            return normalized ? direction.normalized : direction;
        }

        #endregion

        #region Hierarchy Queries

        /// <summary>
        /// Checks if this transform is a child (at any depth) of the specified parent.
        /// </summary>
        /// <param name="transform">The transform to check</param>
        /// <param name="parent">The potential parent transform</param>
        /// <returns>True if this transform is a descendant of the parent</returns>
        public static bool IsChildOf(this Transform transform, Transform parent)
        {
            if (transform == null || parent == null)
                return false;

            return transform.IsChildOf(parent);
        }

        /// <summary>
        /// Gets the full hierarchy path of the transform.
        /// </summary>
        /// <param name="transform">The transform to get the path for</param>
        /// <returns>A string representing the full path (e.g., "Parent/Child/GrandChild")</returns>
        public static string GetPath(this Transform transform)
        {
            if (transform == null)
                return string.Empty;

            string path = transform.name;
            Transform current = transform.parent;

            while (current != null)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

        /// <summary>
        /// Finds a child transform by name at any depth in the hierarchy.
        /// </summary>
        /// <param name="transform">The parent transform</param>
        /// <param name="name">The name of the child to find</param>
        /// <param name="includeInactive">Whether to include inactive children in the search</param>
        /// <returns>The found transform, or null if not found</returns>
        public static Transform FindDeep(this Transform transform, string name, bool includeInactive = true)
        {
            if (transform == null || string.IsNullOrEmpty(name))
                return null;

            // Check direct children first
            Transform result = transform.Find(name);
            if (result != null)
                return result;

            // Recursively search children
            foreach (Transform child in transform)
            {
                if (!includeInactive && !child.gameObject.activeInHierarchy)
                    continue;

                result = child.FindDeep(name, includeInactive);
                if (result != null)
                    return result;
            }

            return null;
        }

        #endregion
    }
}
