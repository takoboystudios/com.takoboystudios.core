using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Extension methods for LayerMask providing utilities for layer checking and manipulation.
    /// </summary>
    public static class LayerMaskExtensions
    {
        #region Layer Checking

        /// <summary>
        /// Checks if the LayerMask includes the specified layer.
        /// </summary>
        /// <param name="layerMask">The LayerMask to check</param>
        /// <param name="layer">The layer number to check for (0-31)</param>
        /// <returns>True if the layer is included in the mask</returns>
        /// <remarks>
        /// This method performs a bitwise check to determine if a specific layer
        /// is included in the LayerMask. Layer numbers range from 0 to 31.
        /// </remarks>
        /// <example>
        /// <code>
        /// LayerMask groundMask = LayerMask.GetMask("Ground", "Floor");
        /// int groundLayer = LayerMask.NameToLayer("Ground");
        /// 
        /// if (groundMask.IncludesLayer(groundLayer))
        /// {
        ///     Debug.Log("Ground layer is included in the mask");
        /// }
        /// </code>
        /// </example>
        public static bool IncludesLayer(this LayerMask layerMask, int layer)
        {
            if (layer < 0 || layer > 31)
            {
                Debug.LogWarning($"[LayerMaskExtensions] Layer {layer} is out of valid range (0-31)");
                return false;
            }

            return ((1 << layer) & layerMask.value) != 0;
        }

        /// <summary>
        /// Checks if the LayerMask includes the specified layer by name.
        /// </summary>
        /// <param name="layerMask">The LayerMask to check</param>
        /// <param name="layerName">The name of the layer to check for</param>
        /// <returns>True if the layer is included in the mask</returns>
        /// <example>
        /// <code>
        /// LayerMask groundMask = LayerMask.GetMask("Ground", "Floor");
        /// 
        /// if (groundMask.IncludesLayer("Ground"))
        /// {
        ///     Debug.Log("Ground layer is included in the mask");
        /// }
        /// </code>
        /// </example>
        public static bool IncludesLayer(this LayerMask layerMask, string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogWarning("[LayerMaskExtensions] Cannot check for null or empty layer name");
                return false;
            }

            int layer = LayerMask.NameToLayer(layerName);
            
            if (layer == -1)
            {
                Debug.LogWarning($"[LayerMaskExtensions] Layer '{layerName}' does not exist");
                return false;
            }

            return IncludesLayer(layerMask, layer);
        }

        /// <summary>
        /// Checks if the LayerMask includes the layer of the specified GameObject.
        /// </summary>
        /// <param name="layerMask">The LayerMask to check</param>
        /// <param name="gameObject">The GameObject whose layer to check</param>
        /// <returns>True if the GameObject's layer is included in the mask</returns>
        /// <example>
        /// <code>
        /// LayerMask interactableMask = LayerMask.GetMask("Interactable");
        /// 
        /// if (interactableMask.IncludesLayer(targetObject))
        /// {
        ///     // Interact with the object
        /// }
        /// </code>
        /// </example>
        public static bool IncludesLayer(this LayerMask layerMask, GameObject gameObject)
        {
            if (gameObject == null)
            {
                Debug.LogWarning("[LayerMaskExtensions] Cannot check layer for null GameObject");
                return false;
            }

            return IncludesLayer(layerMask, gameObject.layer);
        }

        #endregion

        #region Layer Manipulation

        /// <summary>
        /// Adds a layer to the LayerMask.
        /// </summary>
        /// <param name="layerMask">The LayerMask to modify</param>
        /// <param name="layer">The layer number to add (0-31)</param>
        /// <returns>A new LayerMask with the layer added</returns>
        public static LayerMask AddLayer(this LayerMask layerMask, int layer)
        {
            if (layer < 0 || layer > 31)
            {
                Debug.LogWarning($"[LayerMaskExtensions] Layer {layer} is out of valid range (0-31)");
                return layerMask;
            }

            return layerMask | (1 << layer);
        }

        /// <summary>
        /// Adds a layer to the LayerMask by name.
        /// </summary>
        /// <param name="layerMask">The LayerMask to modify</param>
        /// <param name="layerName">The name of the layer to add</param>
        /// <returns>A new LayerMask with the layer added</returns>
        public static LayerMask AddLayer(this LayerMask layerMask, string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogWarning("[LayerMaskExtensions] Cannot add null or empty layer name");
                return layerMask;
            }

            int layer = LayerMask.NameToLayer(layerName);
            
            if (layer == -1)
            {
                Debug.LogWarning($"[LayerMaskExtensions] Layer '{layerName}' does not exist");
                return layerMask;
            }

            return AddLayer(layerMask, layer);
        }

        /// <summary>
        /// Removes a layer from the LayerMask.
        /// </summary>
        /// <param name="layerMask">The LayerMask to modify</param>
        /// <param name="layer">The layer number to remove (0-31)</param>
        /// <returns>A new LayerMask with the layer removed</returns>
        public static LayerMask RemoveLayer(this LayerMask layerMask, int layer)
        {
            if (layer < 0 || layer > 31)
            {
                Debug.LogWarning($"[LayerMaskExtensions] Layer {layer} is out of valid range (0-31)");
                return layerMask;
            }

            return layerMask & ~(1 << layer);
        }

        /// <summary>
        /// Removes a layer from the LayerMask by name.
        /// </summary>
        /// <param name="layerMask">The LayerMask to modify</param>
        /// <param name="layerName">The name of the layer to remove</param>
        /// <returns>A new LayerMask with the layer removed</returns>
        public static LayerMask RemoveLayer(this LayerMask layerMask, string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogWarning("[LayerMaskExtensions] Cannot remove null or empty layer name");
                return layerMask;
            }

            int layer = LayerMask.NameToLayer(layerName);
            
            if (layer == -1)
            {
                Debug.LogWarning($"[LayerMaskExtensions] Layer '{layerName}' does not exist");
                return layerMask;
            }

            return RemoveLayer(layerMask, layer);
        }

        /// <summary>
        /// Toggles a layer in the LayerMask (adds if not present, removes if present).
        /// </summary>
        /// <param name="layerMask">The LayerMask to modify</param>
        /// <param name="layer">The layer number to toggle (0-31)</param>
        /// <returns>A new LayerMask with the layer toggled</returns>
        public static LayerMask ToggleLayer(this LayerMask layerMask, int layer)
        {
            if (layer < 0 || layer > 31)
            {
                Debug.LogWarning($"[LayerMaskExtensions] Layer {layer} is out of valid range (0-31)");
                return layerMask;
            }

            return layerMask ^ (1 << layer);
        }

        /// <summary>
        /// Toggles a layer in the LayerMask by name.
        /// </summary>
        /// <param name="layerMask">The LayerMask to modify</param>
        /// <param name="layerName">The name of the layer to toggle</param>
        /// <returns>A new LayerMask with the layer toggled</returns>
        public static LayerMask ToggleLayer(this LayerMask layerMask, string layerName)
        {
            if (string.IsNullOrEmpty(layerName))
            {
                Debug.LogWarning("[LayerMaskExtensions] Cannot toggle null or empty layer name");
                return layerMask;
            }

            int layer = LayerMask.NameToLayer(layerName);
            
            if (layer == -1)
            {
                Debug.LogWarning($"[LayerMaskExtensions] Layer '{layerName}' does not exist");
                return layerMask;
            }

            return ToggleLayer(layerMask, layer);
        }

        #endregion

        #region Utility

        /// <summary>
        /// Inverts the LayerMask (all included layers become excluded and vice versa).
        /// </summary>
        /// <param name="layerMask">The LayerMask to invert</param>
        /// <returns>A new inverted LayerMask</returns>
        public static LayerMask Invert(this LayerMask layerMask)
        {
            return ~layerMask;
        }

        /// <summary>
        /// Checks if the LayerMask is empty (includes no layers).
        /// </summary>
        /// <param name="layerMask">The LayerMask to check</param>
        /// <returns>True if the mask includes no layers</returns>
        public static bool IsEmpty(this LayerMask layerMask)
        {
            return layerMask.value == 0;
        }

        /// <summary>
        /// Checks if the LayerMask includes all layers.
        /// </summary>
        /// <param name="layerMask">The LayerMask to check</param>
        /// <returns>True if the mask includes all 32 layers</returns>
        public static bool IsEverything(this LayerMask layerMask)
        {
            return layerMask.value == -1;
        }

        /// <summary>
        /// Gets the number of layers included in the LayerMask.
        /// </summary>
        /// <param name="layerMask">The LayerMask to count</param>
        /// <returns>The number of layers included in the mask</returns>
        public static int GetLayerCount(this LayerMask layerMask)
        {
            int count = 0;
            int value = layerMask.value;

            // Count set bits
            while (value != 0)
            {
                count += value & 1;
                value >>= 1;
            }

            return count;
        }

        /// <summary>
        /// Gets an array of all layer numbers included in the LayerMask.
        /// </summary>
        /// <param name="layerMask">The LayerMask to examine</param>
        /// <returns>An array of layer numbers (0-31) included in the mask</returns>
        public static int[] GetLayers(this LayerMask layerMask)
        {
            System.Collections.Generic.List<int> layers = new System.Collections.Generic.List<int>();

            for (int i = 0; i < 32; i++)
            {
                if (layerMask.IncludesLayer(i))
                {
                    layers.Add(i);
                }
            }

            return layers.ToArray();
        }

        /// <summary>
        /// Gets an array of all layer names included in the LayerMask.
        /// </summary>
        /// <param name="layerMask">The LayerMask to examine</param>
        /// <returns>An array of layer names included in the mask</returns>
        public static string[] GetLayerNames(this LayerMask layerMask)
        {
            System.Collections.Generic.List<string> layerNames = new System.Collections.Generic.List<string>();

            for (int i = 0; i < 32; i++)
            {
                if (layerMask.IncludesLayer(i))
                {
                    string layerName = LayerMask.LayerToName(i);
                    if (!string.IsNullOrEmpty(layerName))
                    {
                        layerNames.Add(layerName);
                    }
                }
            }

            return layerNames.ToArray();
        }

        #endregion
    }
}
