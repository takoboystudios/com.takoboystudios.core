using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Configuration settings for an object pool.
    /// </summary>
    [Serializable]
    public class PoolConfig
    {
        /// <summary>
        /// Initial number of objects to pre-instantiate in the pool.
        /// </summary>
        [Tooltip("Number of objects to create when the pool is initialized")]
        public int initialCapacity = 10;

        /// <summary>
        /// Maximum number of objects the pool can hold.
        /// </summary>
        [Tooltip("Maximum pool size. Objects beyond this limit will be destroyed instead of pooled")]
        public int maxCapacity = 100;

        /// <summary>
        /// Number of objects to create when the pool runs out and needs to grow.
        /// </summary>
        [Tooltip("How many objects to create at once when pool is empty")]
        public int growSize = 5;

        /// <summary>
        /// If true, the pool will automatically grow when empty. If false, returns null when empty.
        /// </summary>
        [Tooltip("Whether the pool should auto-create objects when empty")]
        public bool autoGrow = true;

        /// <summary>
        /// If true, validates that released objects haven't already been released (helps catch bugs).
        /// </summary>
        [Tooltip("Enable collection checks to catch double-release bugs (has performance cost)")]
        public bool collectionCheck = true;

        /// <summary>
        /// Creates a default pool configuration.
        /// </summary>
        public PoolConfig() { }

        /// <summary>
        /// Creates a pool configuration with specified values.
        /// </summary>
        public PoolConfig(int initial, int max, int grow = 5, bool autoGrow = true, bool collectionCheck = true)
        {
            initialCapacity = initial;
            maxCapacity = max;
            growSize = grow;
            this.autoGrow = autoGrow;
            this.collectionCheck = collectionCheck;
        }
    }

    /// <summary>
    /// Centralized manager for GameObject pooling using Unity's ObjectPool under the hood.
    /// Provides named pools, async support, pre-warming, and automatic lifecycle management.
    /// </summary>
    /// <remarks>
    /// The PoolManager wraps Unity's high-performance ObjectPool while adding:
    /// - Named pool management (one manager, multiple pools)
    /// - Async Addressables support
    /// - Pre-warming on creation
    /// - Auto-grow when empty
    /// - GameObject convenience methods (position, parent, etc.)
    /// - IPoolable lifecycle callbacks
    /// - Comprehensive diagnostics and stats
    /// 
    /// This manager is a singleton that persists across scenes.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a pool
    /// PoolManager.Instance.CreatePool("Bullet", bulletPrefab, new PoolConfig(20, 100));
    /// 
    /// // Get from pool
    /// GameObject bullet = PoolManager.Instance.Get("Bullet", spawnPoint.position);
    /// 
    /// // Return to pool
    /// PoolManager.Instance.Release(bullet);
    /// 
    /// // Or use extension method
    /// bullet.ReturnToPool();
    /// </code>
    /// </example>
    public class PoolManager : MonoBehaviour
    {
        #region Singleton

        private static PoolManager _instance;

        /// <summary>
        /// Gets the singleton instance of the PoolManager.
        /// </summary>
        public static PoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PoolManager>();

                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[PoolManager]");
                        _instance = go.AddComponent<PoolManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Private Fields

        private readonly Dictionary<string, ObjectPool<GameObject>> _pools = new Dictionary<string, ObjectPool<GameObject>>();
        private readonly Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, PoolConfig> _configs = new Dictionary<string, PoolConfig>();
        private readonly Dictionary<GameObject, string> _instanceToPoolName = new Dictionary<GameObject, string>();
        private readonly Dictionary<string, PoolStatistics> _statistics = new Dictionary<string, PoolStatistics>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                ClearAllPools();
                _instance = null;
            }
        }

        private void OnApplicationQuit()
        {
            ClearAllPools();
        }

        #endregion

        #region Pool Creation

        /// <summary>
        /// Creates a new pool for the specified prefab with default configuration.
        /// </summary>
        /// <param name="poolName">Unique name for this pool</param>
        /// <param name="prefab">The prefab to pool</param>
        /// <param name="preWarmCount">Number of instances to pre-create (default: 10)</param>
        /// <returns>True if pool was created successfully</returns>
        public bool CreatePool(GameObject prefab, int preWarmCount = 10)
        {
            return CreatePool(prefab.name, prefab, new PoolConfig(preWarmCount, 100));
        }

        /// <summary>
        /// Creates a new pool for the specified prefab with custom configuration.
        /// </summary>
        /// <param name="poolName">Unique name for this pool</param>
        /// <param name="prefab">The prefab to pool</param>
        /// <param name="config">Pool configuration settings</param>
        /// <returns>True if pool was created successfully</returns>
        public bool CreatePool(string poolName, GameObject prefab, PoolConfig config)
        {
            if (string.IsNullOrEmpty(poolName))
            {
                Debug.LogError("[PoolManager] Pool name cannot be null or empty");
                return false;
            }

            if (prefab == null)
            {
                Debug.LogError($"[PoolManager] Prefab for pool '{poolName}' cannot be null");
                return false;
            }

            if (_pools.ContainsKey(poolName))
            {
                Debug.LogWarning($"[PoolManager] Pool '{poolName}' already exists. Skipping creation.");
                return false;
            }

            try
            {
                // Store prefab and config
                _prefabs[poolName] = prefab;
                _configs[poolName] = config;
                _statistics[poolName] = new PoolStatistics();

                // Create Unity ObjectPool
                var pool = new ObjectPool<GameObject>(
                    createFunc: () => CreateInstance(poolName),
                    actionOnGet: (obj) => OnGetFromPool(poolName, obj),
                    actionOnRelease: (obj) => OnReleaseToPool(poolName, obj),
                    actionOnDestroy: (obj) => OnDestroyPoolObject(poolName, obj),
                    collectionCheck: config.collectionCheck,
                    defaultCapacity: config.initialCapacity,
                    maxSize: config.maxCapacity
                );

                _pools[poolName] = pool;

                // Pre-warm the pool
                if (config.initialCapacity > 0)
                {
                    PreWarm(poolName, config.initialCapacity);
                }

                Debug.Log($"[PoolManager] Created pool '{poolName}' with capacity {config.initialCapacity}/{config.maxCapacity}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PoolManager] Failed to create pool '{poolName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Creates a pool asynchronously, useful when loading prefabs from Addressables.
        /// </summary>
        /// <param name="poolName">Unique name for this pool</param>
        /// <param name="loadPrefabFunc">Async function that loads and returns the prefab</param>
        /// <param name="config">Pool configuration settings</param>
        /// <returns>True if pool was created successfully</returns>
        public async UniTask<bool> CreatePoolAsync(string poolName, Func<UniTask<GameObject>> loadPrefabFunc, PoolConfig config = null)
        {
            if (loadPrefabFunc == null)
            {
                Debug.LogError("[PoolManager] Load prefab function cannot be null");
                return false;
            }

            try
            {
                GameObject prefab = await loadPrefabFunc();

                if (prefab == null)
                {
                    Debug.LogError($"[PoolManager] Failed to load prefab for pool '{poolName}'");
                    return false;
                }

                return CreatePool(poolName, prefab, config ?? new PoolConfig());
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PoolManager] Failed to create pool '{poolName}' async: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Pool Operations

        /// <summary>
        /// Gets an object from the pool at the origin.
        /// </summary>
        /// <param name="poolName">Name of the pool to get from</param>
        /// <returns>A pooled GameObject, or null if pool doesn't exist or can't create instance</returns>
        public GameObject Acquire(string poolName)
        {
            return Acquire(poolName, Vector3.zero, Quaternion.identity, null);
        }

        /// <summary>
        /// Gets an object from the pool at the specified position.
        /// </summary>
        /// <param name="poolName">Name of the pool to get from</param>
        /// <param name="position">World position to place the object</param>
        /// <returns>A pooled GameObject, or null if pool doesn't exist or can't create instance</returns>
        public GameObject Acquire(string poolName, Vector3 position)
        {
            return Acquire(poolName, position, Quaternion.identity, null);
        }

        /// <summary>
        /// Gets an object from the pool with position and rotation.
        /// </summary>
        /// <param name="poolName">Name of the pool to get from</param>
        /// <param name="position">World position to place the object</param>
        /// <param name="rotation">World rotation to apply to the object</param>
        /// <returns>A pooled GameObject, or null if pool doesn't exist or can't create instance</returns>
        public GameObject Acquire(string poolName, Vector3 position, Quaternion rotation)
        {
            return Acquire(poolName, position, rotation, null);
        }

        /// <summary>
        /// Gets an object from the pool with position, rotation, and parent.
        /// </summary>
        /// <param name="poolName">Name of the pool to get from</param>
        /// <param name="position">World position to place the object</param>
        /// <param name="rotation">World rotation to apply to the object</param>
        /// <param name="parent">Parent transform (can be null)</param>
        /// <returns>A pooled GameObject, or null if pool doesn't exist or can't create instance</returns>
        public GameObject Acquire(string poolName, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"[PoolManager] Pool '{poolName}' does not exist. Create it first using CreatePool().");
                return null;
            }

            try
            {
                GameObject obj = pool.Get();

                if (obj == null)
                {
                    Debug.LogError($"[PoolManager] Failed to get object from pool '{poolName}'");
                    return null;
                }

                // Set transform
                obj.transform.SetParent(parent);
                obj.transform.position = position;
                obj.transform.rotation = rotation;

                // Track statistics
                _statistics[poolName].totalAcquired++;
                _statistics[poolName].currentActive++;

                return obj;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PoolManager] Error getting object from pool '{poolName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Attempts to get an object from the pool without throwing exceptions.
        /// </summary>
        /// <param name="poolName">Name of the pool to get from</param>
        /// <param name="result">The retrieved object, or null if failed</param>
        /// <returns>True if object was successfully retrieved</returns>
        public bool TryAcquire(string poolName, out GameObject result)
        {
            result = Acquire(poolName);
            return result != null;
        }

        /// <summary>
        /// Returns an object to its pool.
        /// </summary>
        /// <param name="obj">The GameObject to return</param>
        /// <returns>True if successfully returned to pool</returns>
        public bool Release(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("[PoolManager] Cannot release null object");
                return false;
            }

            if (!_instanceToPoolName.TryGetValue(obj, out string poolName))
            {
                Debug.LogWarning($"[PoolManager] Object '{obj.name}' does not belong to any pool. Destroying instead.");
                Destroy(obj);
                return false;
            }

            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"[PoolManager] Pool '{poolName}' no longer exists. Destroying object.");
                Destroy(obj);
                return false;
            }

            try
            {
                pool.Release(obj);

                // Track statistics
                _statistics[poolName].totalReleased++;
                _statistics[poolName].currentActive--;

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PoolManager] Error releasing object to pool '{poolName}': {ex.Message}");
                Destroy(obj);
                return false;
            }
        }

        #endregion

        #region Pool Management

        /// <summary>
        /// Pre-warms a pool by creating the specified number of instances.
        /// </summary>
        /// <param name="poolName">Name of the pool to pre-warm</param>
        /// <param name="count">Number of instances to create</param>
        public void PreWarm(string poolName, int count)
        {
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogError($"[PoolManager] Cannot pre-warm non-existent pool '{poolName}'");
                return;
            }

            var temp = new List<GameObject>(count);

            for (int i = 0; i < count; i++)
            {
                temp.Add(pool.Get());
            }

            foreach (var obj in temp)
            {
                pool.Release(obj);
            }

            Debug.Log($"[PoolManager] Pre-warmed pool '{poolName}' with {count} instances");
        }

        /// <summary>
        /// Clears a specific pool, destroying all its objects.
        /// </summary>
        /// <param name="poolName">Name of the pool to clear</param>
        public void ClearPool(string poolName)
        {
            if (!_pools.TryGetValue(poolName, out var pool))
            {
                Debug.LogWarning($"[PoolManager] Cannot clear non-existent pool '{poolName}'");
                return;
            }

            pool.Clear();
            _pools.Remove(poolName);
            _prefabs.Remove(poolName);
            _configs.Remove(poolName);
            _statistics.Remove(poolName);

            // Clean up instance tracking
            var keysToRemove = new List<GameObject>();
            foreach (var kvp in _instanceToPoolName)
            {
                if (kvp.Value == poolName)
                    keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
            {
                _instanceToPoolName.Remove(key);
            }

            Debug.Log($"[PoolManager] Cleared pool '{poolName}'");
        }

        /// <summary>
        /// Clears all pools, destroying all pooled objects.
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var poolName in new List<string>(_pools.Keys))
            {
                ClearPool(poolName);
            }

            Debug.Log("[PoolManager] Cleared all pools");
        }

        /// <summary>
        /// Checks if a pool exists.
        /// </summary>
        /// <param name="poolName">Name of the pool to check</param>
        /// <returns>True if the pool exists</returns>
        public bool HasPool(string poolName)
        {
            return _pools.ContainsKey(poolName);
        }

        /// <summary>
        /// Gets the names of all active pools.
        /// </summary>
        /// <returns>Array of pool names</returns>
        public string[] GetPoolNames()
        {
            var names = new string[_pools.Count];
            _pools.Keys.CopyTo(names, 0);
            return names;
        }

        #endregion

        #region Statistics & Diagnostics

        /// <summary>
        /// Gets statistics for a specific pool.
        /// </summary>
        /// <param name="poolName">Name of the pool</param>
        /// <returns>Pool statistics, or null if pool doesn't exist</returns>
        public PoolStatistics GetStatistics(string poolName)
        {
            return _statistics.TryGetValue(poolName, out var stats) ? stats : null;
        }

        /// <summary>
        /// Logs statistics for all pools to the console.
        /// </summary>
        public void LogAllStatistics()
        {
            Debug.Log("=== Pool Manager Statistics ===");

            foreach (var kvp in _statistics)
            {
                var stats = kvp.Value;
                Debug.Log($"Pool '{kvp.Key}':\n" +
                         $"  Created: {stats.totalCreated}\n" +
                         $"  Acquired: {stats.totalAcquired}\n" +
                         $"  Released: {stats.totalReleased}\n" +
                         $"  Destroyed: {stats.totalDestroyed}\n" +
                         $"  Active: {stats.currentActive}");
            }
        }

        #endregion

        #region Internal Callbacks

        /// <summary>
        /// Called when creating a new instance for the pool.
        /// </summary>
        private GameObject CreateInstance(string poolName)
        {
            if (!_prefabs.TryGetValue(poolName, out var prefab))
            {
                Debug.LogError($"[PoolManager] No prefab found for pool '{poolName}'");
                return null;
            }

            GameObject instance = Instantiate(prefab, transform);
            instance.name = $"{poolName}_Instance";

            // Track which pool this instance belongs to
            _instanceToPoolName[instance] = poolName;

            // Call IPoolable.OnCreated
            var poolables = instance.GetComponents<IPoolable>();
            foreach (var poolable in poolables)
            {
                try
                {
                    poolable.OnCreated();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PoolManager] Error in OnCreated for '{poolName}': {ex.Message}");
                }
            }

            // Track statistics
            _statistics[poolName].totalCreated++;

            return instance;
        }

        /// <summary>
        /// Called when an object is retrieved from the pool.
        /// </summary>
        private void OnGetFromPool(string poolName, GameObject obj)
        {
            // Call IPoolable.OnAcquired
            var poolables = obj.GetComponents<IPoolable>();
            foreach (var poolable in poolables)
            {
                try
                {
                    poolable.OnAcquired();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PoolManager] Error in OnAcquired for '{poolName}': {ex.Message}");
                }
            }

            obj.SetActive(true);
        }

        /// <summary>
        /// Called when an object is returned to the pool.
        /// </summary>
        private void OnReleaseToPool(string poolName, GameObject obj)
        {
            if (obj == null)
                return;

            // Call IPoolable.OnReleased
            var poolables = obj.GetComponents<IPoolable>();
            foreach (var poolable in poolables)
            {
                try
                {
                    poolable.OnReleased();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PoolManager] Error in OnReleased for '{poolName}': {ex.Message}");
                }
            }

            // Reset transform
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            obj.SetActive(false);
        }

        /// <summary>
        /// Called when an object is permanently destroyed.
        /// </summary>
        private void OnDestroyPoolObject(string poolName, GameObject obj)
        {
            if (obj == null)
                return;

            // Call IPoolable.OnDestroyed
            var poolables = obj.GetComponents<IPoolable>();
            foreach (var poolable in poolables)
            {
                try
                {
                    poolable.OnDestroyed();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[PoolManager] Error in OnDestroyed for '{poolName}': {ex.Message}");
                }
            }

            // Remove from tracking
            _instanceToPoolName.Remove(obj);

            // Track statistics
            if (_statistics.ContainsKey(poolName))
            {
                _statistics[poolName].totalDestroyed++;
            }

            Destroy(obj);
        }

        #endregion
    }

    /// <summary>
    /// Statistics tracking for a pool.
    /// </summary>
    public class PoolStatistics
    {
        /// <summary>
        /// Total number of objects created for this pool.
        /// </summary>
        public int totalCreated;

        /// <summary>
        /// Total number of times objects were acquired from this pool.
        /// </summary>
        public int totalAcquired;

        /// <summary>
        /// Total number of times objects were released to this pool.
        /// </summary>
        public int totalReleased;

        /// <summary>
        /// Total number of objects destroyed from this pool.
        /// </summary>
        public int totalDestroyed;

        /// <summary>
        /// Current number of objects active (not in pool).
        /// </summary>
        public int currentActive;
    }
}
