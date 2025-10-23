using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Object = UnityEngine.Object;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Thread-safe static loader for managing Addressable GameObject instances throughout their lifecycle.
    /// Provides automatic memory leak detection, proper resource cleanup, and comprehensive error handling.
    /// </summary>
    /// <remarks>
    /// This loader ensures that all instantiated Addressable objects are properly tracked and released
    /// to prevent memory leaks. Each instantiated object receives an AddressablesInstanceTracker component
    /// that will log warnings if the object is destroyed without proper cleanup.
    /// 
    /// Usage:
    /// - Use Load() to instantiate Addressable prefabs
    /// - Use Unload() to properly release instances when done
    /// - Use UnloadAllAssets() during scene transitions or application shutdown
    /// </remarks>
    public static class AddressableReferenceLoader
    {
        #region Private Fields

        /// <summary>
        /// Tracks loaded asset handles by their addressable key.
        /// Currently unused but maintained for future caching implementation.
        /// </summary>
        private static readonly Dictionary<string, AsyncOperationHandle<GameObject>> _assetHandles =
            new Dictionary<string, AsyncOperationHandle<GameObject>>();

        /// <summary>
        /// Tracks in-progress loading operations to prevent duplicate loads of the same asset.
        /// Currently unused but maintained for future load deduplication.
        /// </summary>
        private static readonly Dictionary<string, UniTask<GameObject>> _loadingTasks =
            new Dictionary<string, UniTask<GameObject>>();

        /// <summary>
        /// Tracks all instantiated GameObjects managed by this loader for proper cleanup.
        /// Thread-safe operations are enforced through locking.
        /// </summary>
        private static readonly HashSet<GameObject> _instantiatedObjects =
            new HashSet<GameObject>();

        /// <summary>
        /// Lock object for thread-safe operations on the instantiated objects collection.
        /// </summary>
        private static readonly object _lockObject = new object();

        #endregion

        #region Public Methods

        /// <summary>
        /// Asynchronously loads and instantiates a GameObject from Addressables.
        /// </summary>
        /// <param name="key">The Addressable key or address of the prefab to load</param>
        /// <param name="parent">The parent Transform to instantiate under. Pass null for root level.</param>
        /// <param name="startsInactive">If true, the instantiated GameObject will be inactive initially</param>
        /// <returns>The instantiated GameObject, or null if loading failed</returns>
        /// <exception cref="ArgumentNullException">Thrown if key is null or empty</exception>
        /// <remarks>
        /// The returned GameObject will have an AddressablesInstanceTracker component attached
        /// for automatic leak detection. Always call Unload() when done with the instance.
        /// 
        /// Example:
        /// <code>
        /// GameObject instance = await AddressableReferenceLoader.Load("Prefabs/MyPrefab", transform);
        /// if (instance != null)
        /// {
        ///     // Use the instance
        /// }
        /// </code>
        /// </remarks>
        public static async UniTask<GameObject> Load(
            string key,
            Transform parent,
            bool startsInactive = false
        )
        {
            // Validate input
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[AddressableLoader] Cannot load asset: key is null or empty");
                return null;
            }

            // Capture stack trace in editor for better debugging
#if UNITY_EDITOR
            string stackTrace = new System.Diagnostics.StackTrace(true).ToString();
#endif

            AsyncOperationHandle<GameObject> handle = default;
            
            try
            {
                // Initiate the instantiation operation
                handle = Addressables.InstantiateAsync(key, parent);
                
                // Await completion using UniTask for better performance
                await handle.ToUniTask();

                // Validate the operation completed successfully
                if (!handle.IsValid())
                {
                    Debug.LogError($"[AddressableLoader] Invalid handle returned for key: {key}");
                    return null;
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    Debug.LogError($"[AddressableLoader] Failed to instantiate prefab: {key} | Status: {handle.Status}");
                    
                    // Log the operation exception if available
                    if (handle.OperationException != null)
                    {
                        Debug.LogError($"[AddressableLoader] Operation exception: {handle.OperationException}");
                    }
                    
                    return null;
                }

                GameObject instance = handle.Result;
                
                // Validate the instance is not null
                if (instance == null)
                {
#if UNITY_EDITOR
                    Debug.LogError(
                        $"[AddressableLoader] Null result from instantiating key: {key}\n" +
                        $"This may indicate the key exists but the prefab is empty or invalid.\n{stackTrace}"
                    );
#else
                    Debug.LogError(
                        $"[AddressableLoader] Null result from instantiating key: {key}\n" +
                        "This may indicate the key exists but the prefab is empty or invalid."
                    );
#endif
                    return null;
                }

                // Configure the instance
                instance.SetActive(!startsInactive);
                
                // Add tracker component for memory leak detection
                if (!instance.TryGetComponent<AddressablesInstanceTracker>(out _))
                {
                    instance.AddComponent<AddressablesInstanceTracker>();
                }

                // Register the instance for tracking (thread-safe)
                lock (_lockObject)
                {
                    _instantiatedObjects.Add(instance);
                }

                Debug.Log($"[AddressableLoader] Successfully loaded: {key} → {instance.name}");
                return instance;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressableLoader] Exception while loading {key}: {ex.Message}\n{ex.StackTrace}");
                
                // Clean up the handle if it was created
                if (handle.IsValid())
                {
                    try
                    {
                        Addressables.Release(handle);
                    }
                    catch (Exception releaseEx)
                    {
                        Debug.LogWarning($"[AddressableLoader] Failed to release handle after load failure: {releaseEx.Message}");
                    }
                }
                
                return null;
            }
        }

        /// <summary>
        /// Immediately releases an instantiated GameObject and disposes of all IDisposable components.
        /// </summary>
        /// <param name="instance">The GameObject instance to unload. Can be null (no-op).</param>
        /// <remarks>
        /// This method is safe to call multiple times on the same instance. It will:
        /// 1. Mark the instance as unloaded in the tracker
        /// 2. Dispose all IDisposable components
        /// 3. Release the instance back to the Addressables system
        /// 4. Remove it from internal tracking
        /// 
        /// Always call this method instead of Object.Destroy() for Addressable instances.
        /// </remarks>
        public static void Unload(GameObject instance)
        {
            // Early exits for invalid states
            if (!Application.isPlaying)
                return;

            if (instance == null)
                return;

            // Mark the tracker to prevent duplicate unload warnings
            if (instance.TryGetComponent(out AddressablesInstanceTracker tracker))
            {
                tracker.MarkUnloaded();
            }

            // Dispose all IDisposable components for proper cleanup
            foreach (var disposable in instance.GetComponents<IDisposable>())
            {
                try
                {
                    disposable.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning(
                        $"[AddressableLoader] Exception while disposing component on '{instance.name}': {ex.Message}"
                    );
                }
            }

            // Check if this instance is tracked by the loader
            bool isTracked;
            lock (_lockObject)
            {
                isTracked = _instantiatedObjects.Contains(instance);
            }

            if (!isTracked)
            {
                // This object wasn't loaded through this loader, fall back to Unity destroy
                Debug.LogWarning(
                    $"[AddressableLoader] Unloading untracked instance '{instance.name}'. " +
                    "This object may not have been loaded through AddressableReferenceLoader. Using fallback destroy."
                );
                Object.Destroy(instance);
                return;
            }

            // Attempt to release through Addressables system
            try
            {
                bool released = Addressables.ReleaseInstance(instance);
                
                if (!released)
                {
                    Debug.LogError(
                        $"[AddressableLoader] Failed to release addressable instance: {instance.name}\n" +
                        "The instance may have already been released or was not loaded through Addressables."
                    );
                }
                else
                {
                    Debug.Log($"[AddressableLoader] Successfully unloaded: {instance.name}");
                }
            }
            catch (Exception ex)
            {
                // Last resort fallback to Unity destroy
                Debug.LogWarning(
                    $"[AddressableLoader] ReleaseInstance failed for '{instance.name}': {ex.Message}\n" +
                    "Falling back to Unity destroy. This may indicate a memory leak."
                );
                
                Object.Destroy(instance);
            }
            finally
            {
                // Always remove from tracking (thread-safe)
                lock (_lockObject)
                {
                    _instantiatedObjects.Remove(instance);
                }
            }
        }

        /// <summary>
        /// Asynchronously unloads an instance after a specified delay.
        /// </summary>
        /// <param name="instance">The GameObject instance to unload. Can be null (no-op).</param>
        /// <param name="delaySeconds">The delay in seconds before unloading</param>
        /// <remarks>
        /// Useful for timed effects or delayed cleanup. The instance will be marked as
        /// "unloaded" immediately to prevent memory leak warnings, but the actual release
        /// happens after the delay.
        /// 
        /// Example:
        /// <code>
        /// AddressableReferenceLoader.Unload(particleEffect, 2.0f).Forget();
        /// </code>
        /// </remarks>
        public static async UniTaskVoid Unload(GameObject instance, float delaySeconds)
        {
            // Early exits for invalid states
            if (!Application.isPlaying)
                return;

            if (instance == null)
                return;

            if (delaySeconds < 0)
            {
                Debug.LogWarning(
                    $"[AddressableLoader] Negative delay specified for '{instance.name}'. " +
                    "Unloading immediately."
                );
                Unload(instance);
                return;
            }

            // Mark as unloaded immediately to prevent leak warnings during the delay
            if (instance.TryGetComponent(out AddressablesInstanceTracker tracker))
            {
                tracker.MarkUnloaded();
            }

            try
            {
                // Wait for the specified duration
                await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: instance.GetCancellationTokenOnDestroy());
                
                // Unload after delay
                Unload(instance);
            }
            catch (OperationCanceledException)
            {
                // Object was destroyed before delay completed - this is expected behavior
                Debug.Log($"[AddressableLoader] Delayed unload cancelled for '{instance.name}' (object destroyed)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AddressableLoader] Exception during delayed unload: {ex.Message}");
            }
        }

        /// <summary>
        /// Immediately unloads all cached asset handles and active instances.
        /// </summary>
        /// <remarks>
        /// This is a nuclear option that should be called during:
        /// - Scene transitions
        /// - Application shutdown
        /// - Memory pressure situations
        /// 
        /// After calling this method, all previously loaded instances will be invalid.
        /// Use with caution as it will forcefully release all tracked objects.
        /// </remarks>
        public static void UnloadAllAssets()
        {
            int handleCount = 0;
            int instanceCount = 0;

            // Release all cached asset handles
            foreach (var handle in _assetHandles.Values)
            {
                if (handle.IsValid())
                {
                    try
                    {
                        Addressables.Release(handle);
                        handleCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[AddressableLoader] Asset release error: {ex.Message}");
                    }
                }
            }

            _assetHandles.Clear();

            // Release all instantiated objects (thread-safe iteration)
            GameObject[] instances;
            lock (_lockObject)
            {
                instances = new GameObject[_instantiatedObjects.Count];
                _instantiatedObjects.CopyTo(instances);
                _instantiatedObjects.Clear();
            }

            foreach (var go in instances)
            {
                if (go != null)
                {
                    // Mark as unloaded first
                    if (go.TryGetComponent(out AddressablesInstanceTracker tracker))
                    {
                        tracker.MarkUnloaded();
                    }

                    try
                    {
                        Addressables.ReleaseInstance(go);
                        instanceCount++;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning(
                            $"[AddressableLoader] Instance release error for '{go.name}': {ex.Message}"
                        );
                        
                        // Fallback to Unity destroy
                        Object.Destroy(go);
                    }
                }
            }

            _loadingTasks.Clear();

            Debug.Log(
                $"[AddressableLoader] Unloaded all assets: {handleCount} handles, {instanceCount} instances"
            );
        }

        #endregion

        #region Debug & Diagnostics

        /// <summary>
        /// Logs all currently loaded GameObject references to the console.
        /// </summary>
        /// <remarks>
        /// Useful for debugging memory leaks and tracking asset usage.
        /// Only logs asset handles, not individual instances.
        /// </remarks>
        public static void ShowAllReferences()
        {
            Debug.Log("[AddressableLoader] === Loaded GameObject References ===");
            
            if (_assetHandles.Count == 0)
            {
                Debug.Log("[AddressableLoader] No asset handles cached");
            }
            else
            {
                foreach (var kvp in _assetHandles)
                {
                    string assetName = kvp.Value.IsValid() && kvp.Value.Result != null 
                        ? kvp.Value.Result.name 
                        : "null/invalid";
                    Debug.Log($"[AddressableLoader] Key: '{kvp.Key}' → Asset: '{assetName}'");
                }
            }

            Debug.Log($"[AddressableLoader] Total tracked instances: {InstanceCount()}");
            Debug.Log("[AddressableLoader] =====================================");
        }

        /// <summary>
        /// Gets the current number of tracked instantiated objects.
        /// </summary>
        /// <returns>The count of active instances managed by this loader</returns>
        public static int InstanceCount()
        {
            lock (_lockObject)
            {
                return _instantiatedObjects.Count;
            }
        }

        /// <summary>
        /// Thread-safe method to get the current instance count.
        /// </summary>
        /// <param name="count">The number of tracked instances</param>
        /// <returns>True if the operation succeeded, false if the collection is null (should never happen)</returns>
        public static bool TryGetInstanceCount(out int count)
        {
            lock (_lockObject)
            {
                if (_instantiatedObjects != null)
                {
                    count = _instantiatedObjects.Count;
                    return true;
                }
                
                count = 0;
                return false;
            }
        }

        /// <summary>
        /// Gets diagnostic information about the current state of the loader.
        /// </summary>
        /// <returns>A formatted string containing loader statistics</returns>
        public static string GetDiagnostics()
        {
            lock (_lockObject)
            {
                return $"AddressableReferenceLoader Diagnostics:\n" +
                       $"  Cached Asset Handles: {_assetHandles.Count}\n" +
                       $"  Active Loading Tasks: {_loadingTasks.Count}\n" +
                       $"  Tracked Instances: {_instantiatedObjects.Count}";
            }
        }

        #endregion
    }
}
