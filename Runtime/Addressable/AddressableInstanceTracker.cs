using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Component attached to Addressable GameObject instances to track their lifecycle and detect memory leaks.
    /// Automatically added by AddressableReferenceLoader during instantiation.
    /// </summary>
    /// <remarks>
    /// This component ensures that Addressable assets are properly released through the loader system.
    /// If a GameObject is destroyed without calling AddressableReferenceLoader.Unload(), this tracker
    /// will log a warning and attempt to force cleanup.
    /// 
    /// DO NOT manually add this component - it is automatically managed by AddressableReferenceLoader.
    /// DO NOT manually remove this component - doing so will disable leak detection.
    /// </remarks>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-1000)] // Execute early to catch cleanup before other components
    public class AddressablesInstanceTracker : MonoBehaviour
    {
        #region Private Fields

        /// <summary>
        /// Flag indicating whether this instance has been properly unloaded through the loader system.
        /// Prevents duplicate unload attempts and false positive leak warnings.
        /// </summary>
        private bool _alreadyUnloaded;

        /// <summary>
        /// The time this tracker was created, used for diagnostic purposes.
        /// </summary>
        private float _creationTime;

        /// <summary>
        /// The addressable key if available, used for better error messages.
        /// </summary>
        private string _addressableKey;

#if UNITY_EDITOR
        /// <summary>
        /// Stack trace captured at creation time for debugging leaks in the editor.
        /// </summary>
        private string _creationStackTrace;
#endif

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Called when the component is first created.
        /// </summary>
        private void Awake()
        {
            _creationTime = Time.time;
            
#if UNITY_EDITOR
            // Capture creation stack trace for debugging
            _creationStackTrace = new System.Diagnostics.StackTrace(2, true).ToString();
#endif
        }

        /// <summary>
        /// Called when the GameObject is destroyed.
        /// Detects improper cleanup and attempts to force unload if necessary.
        /// </summary>
        private void OnDestroy()
        {
            // Skip if already properly unloaded
            if (_alreadyUnloaded)
                return;

            // Skip cleanup in edit mode to avoid errors during scene changes
            if (!Application.isPlaying)
                return;

            // Mark as unloaded to prevent recursive calls
            _alreadyUnloaded = true;

            // Calculate how long this instance existed
            float lifetime = Time.time - _creationTime;

            // Log detailed warning about the memory leak
            string objectInfo = string.IsNullOrEmpty(_addressableKey) 
                ? $"'{name}'" 
                : $"'{name}' (Key: {_addressableKey})";

#if UNITY_EDITOR
            Debug.LogWarning(
                $"[MEMORY LEAK DETECTED] Addressable instance {objectInfo} was destroyed " +
                $"without being properly unloaded via AddressableReferenceLoader.Unload().\n" +
                $"Lifetime: {lifetime:F2}s\n" +
                $"Forcing unload now to prevent memory leak.\n" +
                $"Creation stack trace:\n{_creationStackTrace}",
                gameObject
            );
#else
            Debug.LogWarning(
                $"[MEMORY LEAK DETECTED] Addressable instance {objectInfo} was destroyed " +
                $"without being properly unloaded via AddressableReferenceLoader.Unload().\n" +
                $"Lifetime: {lifetime:F2}s | Forcing unload now to prevent memory leak.",
                gameObject
            );
#endif

            // Attempt to force unload through the loader system
            try
            {
                AddressableReferenceLoader.Unload(gameObject);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(
                    $"[CRITICAL] Failed to force unload leaked instance {objectInfo}: {ex.Message}\n" +
                    "This may result in a memory leak. Check your cleanup code.",
                    gameObject
                );
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Marks this instance as properly unloaded, preventing memory leak warnings.
        /// </summary>
        /// <remarks>
        /// This method is called automatically by AddressableReferenceLoader.Unload().
        /// You should not need to call this manually unless implementing custom cleanup logic.
        /// </remarks>
        public void MarkUnloaded()
        {
            if (_alreadyUnloaded)
            {
                Debug.LogWarning(
                    $"[AddressableTracker] Instance '{name}' was already marked as unloaded. " +
                    "This may indicate duplicate unload calls.",
                    gameObject
                );
                return;
            }

            _alreadyUnloaded = true;
            
            // Log successful cleanup in editor for debugging
#if UNITY_EDITOR && ADDRESSABLE_VERBOSE_LOGGING
            float lifetime = Time.time - _creationTime;
            Debug.Log($"[AddressableTracker] Instance '{name}' properly unloaded after {lifetime:F2}s", gameObject);
#endif
        }

        /// <summary>
        /// Sets the addressable key for this instance for better diagnostic messages.
        /// </summary>
        /// <param name="key">The addressable key used to load this instance</param>
        /// <remarks>
        /// This is optional but recommended for better error messages.
        /// Called automatically by AddressableReferenceLoader if implemented there.
        /// </remarks>
        public void SetAddressableKey(string key)
        {
            _addressableKey = key;
        }

        /// <summary>
        /// Checks if this instance has been properly marked as unloaded.
        /// </summary>
        /// <returns>True if the instance has been unloaded, false otherwise</returns>
        public bool IsUnloaded()
        {
            return _alreadyUnloaded;
        }

        /// <summary>
        /// Gets the lifetime of this instance in seconds.
        /// </summary>
        /// <returns>Time in seconds since this tracker was created</returns>
        public float GetLifetime()
        {
            return Time.time - _creationTime;
        }

        /// <summary>
        /// Gets diagnostic information about this tracked instance.
        /// </summary>
        /// <returns>Formatted string with instance details</returns>
        public string GetDiagnostics()
        {
            return $"AddressableInstanceTracker Diagnostics:\n" +
                   $"  GameObject: {name}\n" +
                   $"  Addressable Key: {(_addressableKey ?? "Unknown")}\n" +
                   $"  Unloaded: {_alreadyUnloaded}\n" +
                   $"  Lifetime: {GetLifetime():F2}s\n" +
                   $"  Creation Time: {_creationTime:F2}s";
        }

        #endregion

        #region Editor Utilities

#if UNITY_EDITOR
        /// <summary>
        /// Gets the creation stack trace (editor only).
        /// </summary>
        /// <returns>The stack trace from when this tracker was created</returns>
        public string GetCreationStackTrace()
        {
            return _creationStackTrace ?? "No stack trace available";
        }

        /// <summary>
        /// Editor-only method to manually trigger the leak warning for testing.
        /// </summary>
        [ContextMenu("Simulate Memory Leak Warning")]
        private void SimulateLeakWarning()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("This test only works in Play Mode");
                return;
            }

            _alreadyUnloaded = false;
            OnDestroy();
        }

        /// <summary>
        /// Editor-only method to display diagnostics in the console.
        /// </summary>
        [ContextMenu("Show Diagnostics")]
        private void ShowDiagnostics()
        {
            Debug.Log(GetDiagnostics(), gameObject);
        }
#endif

        #endregion
    }
}
