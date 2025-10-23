# PoolManager - GameObject Pooling System

A production-grade object pooling system wrapping Unity's `ObjectPool<T>` with named pools, async support, and comprehensive lifecycle management.

## 📦 Files

- **IPoolable.cs** - Interface for poolable objects with lifecycle callbacks
- **PoolManager.cs** - Main singleton manager wrapping Unity's ObjectPool
- **PoolExtensions.cs** - Convenient extension methods

## ✨ Features

### Core Pooling
- ✅ **Named pools** - Manage multiple pools with string keys
- ✅ **Unity ObjectPool wrapper** - Leverages Unity's optimized pooling
- ✅ **Collection checks** - Catches double-release bugs
- ✅ **Max capacity** - Enforces pool size limits
- ✅ **Auto-grow** - Automatically creates more objects when empty

### Lifecycle Management
- ✅ **IPoolable interface** - OnCreated, OnAcquired, OnReleased, OnDestroyed callbacks
- ✅ **Automatic reset** - Resets transform on return
- ✅ **Component support** - Works with any number of IPoolable components

### Convenience Features
- ✅ **Extension methods** - Easy-to-use API
- ✅ **Async support** - UniTask integration for Addressables
- ✅ **Pre-warming** - Create objects upfront
- ✅ **Position/rotation/parent** - Set transform on acquire
- ✅ **Delayed return** - Return to pool after delay

### Diagnostics
- ✅ **Statistics tracking** - Monitor created/acquired/released counts
- ✅ **Active count** - See how many objects are in use
- ✅ **Comprehensive logging** - Debug pool behavior

## 🚀 Quick Start

### 1. Basic Setup

```csharp
using TakoBoyStudios.Core;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    
    void Start()
    {
        // Create a pool
        PoolManager.Instance.CreatePool("Bullet", bulletPrefab, preWarmCount: 20);
    }
}
```

### 2. Get from Pool

```csharp
// Get at position
GameObject bullet = PoolManager.Instance.Get("Bullet", spawnPoint.position);

// Get with rotation
GameObject bullet = PoolManager.Instance.Get("Bullet", position, rotation);

// Get with parent
GameObject bullet = PoolManager.Instance.Get("Bullet", position, rotation, transform);
```

### 3. Return to Pool

```csharp
// Method 1: Direct
PoolManager.Instance.Release(bullet);

// Method 2: Extension method (easier!)
bullet.ReturnToPool();

// Method 3: Delayed
bullet.ReturnToPoolAfter(2f); // Returns after 2 seconds
```

## 📖 Usage Examples

### Creating Pools

```csharp
// Simple pool with defaults
PoolManager.Instance.CreatePool("Enemy", enemyPrefab, preWarmCount: 10);

// Custom configuration
var config = new PoolConfig
{
    initialCapacity = 20,
    maxCapacity = 100,
    growSize = 10,
    autoGrow = true,
    collectionCheck = true
};
PoolManager.Instance.CreatePool("Bullet", bulletPrefab, config);
```

### Async Pool Creation (Addressables)

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;

async UniTask CreateBulletPool()
{
    bool success = await PoolManager.Instance.CreatePoolAsync(
        "Bullet",
        async () => 
        {
            var handle = Addressables.LoadAssetAsync<GameObject>("Bullet");
            return await handle.ToUniTask();
        },
        new PoolConfig(20, 100)
    );
    
    if (success)
    {
        Debug.Log("Bullet pool ready!");
    }
}
```

### Implementing IPoolable

```csharp
using TakoBoyStudios.Core;
using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    private Rigidbody rb;
    private TrailRenderer trail;
    private ParticleSystem particles;
    
    // Called once when first created
    public void OnCreated()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
        particles = GetComponent<ParticleSystem>();
    }
    
    // Called each time taken from pool
    public void OnAcquired()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        trail.Clear();
        particles.Play();
    }
    
    // Called when returned to pool
    public void OnReleased()
    {
        rb.velocity = Vector3.zero;
        trail.Clear();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    
    // Called when destroyed
    public void OnDestroyed()
    {
        // Cleanup any resources
    }
}
```

### Practical Example - Bullet System

```csharp
public class Gun : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float bulletLifetime = 5f;
    
    void Start()
    {
        // Create pool once
        PoolManager.Instance.CreatePool("Bullet", bulletPrefab, 30);
    }
    
    public void Fire()
    {
        // Get from pool
        GameObject bullet = PoolManager.Instance.Get(
            "Bullet", 
            firePoint.position, 
            firePoint.rotation
        );
        
        if (bullet != null)
        {
            // Setup bullet
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.velocity = firePoint.forward * bulletSpeed;
            
            // Auto-return after lifetime
            bullet.ReturnToPoolAfter(bulletLifetime);
        }
    }
}
```

### Practical Example - Enemy Spawner

```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int enemiesPerWave = 10;
    
    void Start()
    {
        // Create pools for each enemy type
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            string poolName = $"Enemy_{i}";
            PoolManager.Instance.CreatePool(poolName, enemyPrefabs[i], 5);
        }
    }
    
    public void SpawnWave()
    {
        for (int i = 0; i < enemiesPerWave; i++)
        {
            // Random enemy type
            int enemyIndex = Random.Range(0, enemyPrefabs.Length);
            string poolName = $"Enemy_{enemyIndex}";
            
            // Random spawn position
            Vector3 spawnPos = GetRandomSpawnPosition();
            
            // Get from pool
            GameObject enemy = PoolManager.Instance.Get(poolName, spawnPos);
        }
    }
    
    Vector3 GetRandomSpawnPosition()
    {
        // Your spawn logic
        return Random.insideUnitSphere * 10f;
    }
}
```

### Practical Example - Particle Effects

```csharp
public class EffectsManager : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private GameObject hitEffectPrefab;
    
    void Start()
    {
        PoolManager.Instance.CreatePool("Explosion", explosionPrefab, 5);
        PoolManager.Instance.CreatePool("HitEffect", hitEffectPrefab, 10);
    }
    
    public void PlayExplosion(Vector3 position)
    {
        GameObject explosion = PoolManager.Instance.Get("Explosion", position);
        
        // Auto-return after particle duration
        ParticleSystem ps = explosion.GetComponent<ParticleSystem>();
        explosion.ReturnToPoolAfter(ps.main.duration);
    }
    
    public void PlayHitEffect(Vector3 position, Quaternion rotation)
    {
        GameObject effect = PoolManager.Instance.Get("HitEffect", position, rotation);
        effect.ReturnToPoolAfter(1f);
    }
}
```

## 🔧 Advanced Usage

### Multiple IPoolable Components

You can have multiple components implementing IPoolable on the same GameObject:

```csharp
public class Enemy : MonoBehaviour, IPoolable
{
    public void OnCreated() { /* Setup */ }
    public void OnAcquired() { /* Reset health, AI, etc */ }
    public void OnReleased() { /* Cleanup */ }
    public void OnDestroyed() { /* Final cleanup */ }
}

public class EnemyAI : MonoBehaviour, IPoolable
{
    public void OnCreated() { /* Cache components */ }
    public void OnAcquired() { /* Reset AI state */ }
    public void OnReleased() { /* Stop AI */ }
    public void OnDestroyed() { /* Cleanup */ }
}

// Both components will receive callbacks automatically!
```

### Pool Statistics

```csharp
// Get stats for a pool
PoolStatistics stats = PoolManager.Instance.GetStatistics("Bullet");
Debug.Log($"Created: {stats.totalCreated}");
Debug.Log($"Active: {stats.currentActive}");
Debug.Log($"Acquired: {stats.totalAcquired}");
Debug.Log($"Released: {stats.totalReleased}");

// Log all pool stats
PoolManager.Instance.LogAllStatistics();
```

### Pre-warming

```csharp
// Pre-warm a pool after creation
PoolManager.Instance.PreWarm("Enemy", 50);

// Or specify in config
var config = new PoolConfig
{
    initialCapacity = 50, // Pre-warms automatically
    maxCapacity = 200
};
```

### Pool Management

```csharp
// Check if pool exists
if (PoolManager.Instance.HasPool("Bullet"))
{
    // Pool is ready
}

// Get all pool names
string[] pools = PoolManager.Instance.GetPoolNames();

// Clear a specific pool
PoolManager.Instance.ClearPool("OldEnemy");

// Clear all pools
PoolManager.Instance.ClearAllPools();
```

### Error Handling

```csharp
// Use TryGet for safe acquisition
if (PoolManager.Instance.TryGet("Bullet", out GameObject bullet))
{
    // Successfully got bullet
}
else
{
    // Pool doesn't exist or failed
}

// Extension method handles nulls gracefully
GameObject obj = null;
obj.ReturnToPool(); // Safely returns false, doesn't crash
```

## 🎯 Best Practices

### 1. Create Pools Early
```csharp
void Awake()
{
    // Create pools in Awake or Start
    // DON'T create during gameplay
    PoolManager.Instance.CreatePool("Bullet", bulletPrefab, 20);
}
```

### 2. Always Implement IPoolable for State Reset
```csharp
// BAD - State persists between uses
public class Enemy : MonoBehaviour
{
    private int health = 100; // Never resets!
}

// GOOD - State resets
public class Enemy : MonoBehaviour, IPoolable
{
    private int health;
    
    public void OnAcquired()
    {
        health = 100; // Resets each use
    }
}
```

### 3. Use Extension Methods
```csharp
// Instead of:
PoolManager.Instance.Release(bullet);

// Do this:
bullet.ReturnToPool(); // Cleaner!
```

### 4. Set Appropriate Capacities
```csharp
// Think about your needs:
// - Bullets: High capacity (50-200)
// - Enemies: Medium capacity (10-50)
// - Particles: Medium capacity (10-30)
// - Boss projectiles: Low capacity (5-10)

new PoolConfig
{
    initialCapacity = 20,  // Pre-create this many
    maxCapacity = 100,     // Never exceed this
    growSize = 10          // Create 10 at a time when empty
};
```

### 5. Return Objects Promptly
```csharp
// DON'T keep references around
private GameObject myBullet;

void Fire()
{
    myBullet = PoolManager.Instance.Get("Bullet");
    // myBullet is never returned - LEAK!
}

// DO return when done
void Fire()
{
    GameObject bullet = PoolManager.Instance.Get("Bullet");
    bullet.ReturnToPoolAfter(5f); // Auto-return
}
```

### 6. Handle Pool Destruction
```csharp
void OnDestroy()
{
    // Clear your pools when appropriate
    PoolManager.Instance.ClearPool("LevelSpecificEnemy");
}
```

## 🐛 Troubleshooting

### Objects Not Resetting Between Uses
**Problem:** Objects keep their state from previous use  
**Solution:** Implement `IPoolable.OnAcquired()` and reset all state

### "Pool doesn't exist" Error
**Problem:** Trying to get from a pool that wasn't created  
**Solution:** Always call `CreatePool()` before `Get()`

### Running Out of Pool Objects
**Problem:** Pool hits max capacity  
**Solution:** Increase `maxCapacity` or ensure objects are returned promptly

### Memory Leaks
**Problem:** Objects never return to pool  
**Solution:** Always call `Release()` or use `ReturnToPoolAfter()`

### Collection Check Errors
**Problem:** "Object is already in pool" exception  
**Solution:** You're releasing the same object twice. Check your return logic.

## ⚡ Performance Tips

1. **Pre-warm pools** - Avoid instantiation during gameplay
2. **Set appropriate max sizes** - Prevents unbounded growth
3. **Use collectionCheck = false in production** - Small performance gain (but lose safety)
4. **Reuse pools across scenes** - PoolManager persists via DontDestroyOnLoad
5. **Batch operations** - Create all pools at startup, not on-demand

## 🔄 Migration from Old System

### From ObjectPool<T>
```csharp
// Old
ObjectPool<MyObject> pool = new ObjectPool<MyObject>(...);
var obj = await pool.AcquireObject("key");

// New
PoolManager.Instance.CreatePool("key", prefab);
var obj = PoolManager.Instance.Get("key");
```

### From PoolLoader
```csharp
// Old
PoolLoader.Instance.CreatePool(prefab, 10);
var obj = PoolLoader.Instance.AcquireObject("name", position);
PoolLoader.Instance.DisposeObject(obj);

// New
PoolManager.Instance.CreatePool("name", prefab, 10);
var obj = PoolManager.Instance.Get("name", position);
obj.ReturnToPool();
```

## 📝 Summary

The PoolManager provides a robust, feature-complete pooling solution by wrapping Unity's optimized ObjectPool with:
- Easy-to-use named pool management
- Comprehensive lifecycle callbacks
- Async Addressables support
- Excellent diagnostics and error handling
- Clean extension method API

Perfect for bullets, enemies, particles, UI elements, and any frequently instantiated objects!
