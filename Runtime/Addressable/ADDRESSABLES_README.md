# Addressable Reference Loader - Safe Addressables Management

A production-grade wrapper for Unity's Addressables system with automatic memory leak detection, proper lifecycle management, and comprehensive error handling.

## 📦 Files

- **AddressableReferenceLoader.cs** - Static loader for managing Addressable instances
- **AddressablesInstanceTracker.cs** - Component for tracking lifecycle and detecting leaks

## ✨ Features

### Core Loading
- ✅ **Async loading** - UniTask-based for optimal performance
- ✅ **Automatic instantiation** - Direct GameObject loading
- ✅ **Parent assignment** - Instantiate under specific transforms
- ✅ **Inactive loading** - Start GameObjects inactive if needed

### Memory Safety
- ✅ **Automatic leak detection** - Warns if objects aren't properly unloaded
- ✅ **Instance tracking** - All loaded objects are tracked
- ✅ **Proper cleanup** - Releases Addressable handles correctly
- ✅ **IDisposable support** - Disposes components on unload

### Advanced Features
- ✅ **Delayed unload** - Release objects after a delay
- ✅ **Bulk unload** - Clear all assets at once
- ✅ **Thread-safe** - Locking for concurrent operations
- ✅ **Diagnostics** - Track usage and debug issues
- ✅ **Editor debugging** - Stack traces for leak detection

## 🚨 The Problem This Solves

### Without This System:
```csharp
// WRONG - Memory leak!
var handle = Addressables.InstantiateAsync("Enemy");
GameObject enemy = await handle.ToUniTask();
Destroy(enemy); // ❌ Handle not released = MEMORY LEAK

// WRONG - Forgot to release
var handle = Addressables.InstantiateAsync("Bullet");
GameObject bullet = await handle.ToUniTask();
// Forgot to call Release() = MEMORY LEAK
```

### With This System:
```csharp
// CORRECT - Automatic tracking and leak detection
GameObject enemy = await AddressableReferenceLoader.Load("Enemy", transform);
AddressableReferenceLoader.Unload(enemy); // ✅ Properly released

// Even if you forget to unload:
GameObject bullet = await AddressableReferenceLoader.Load("Bullet", transform);
Destroy(bullet); // ⚠️ Logs warning and auto-cleans up
```

## 🚀 Quick Start

### 1. Basic Loading & Unloading

```csharp
using TakoBoyStudios.Core;
using Cysharp.Threading.Tasks;

public class EnemySpawner : MonoBehaviour
{
    async UniTask SpawnEnemy()
    {
        // Load enemy prefab
        GameObject enemy = await AddressableReferenceLoader.Load(
            "Enemies/Goblin", 
            transform
        );
        
        if (enemy != null)
        {
            // Use the enemy
            enemy.transform.position = spawnPoint.position;
            
            // When done, unload properly
            AddressableReferenceLoader.Unload(enemy);
        }
    }
}
```

### 2. Loading Inactive

```csharp
// Load but start inactive
GameObject obj = await AddressableReferenceLoader.Load(
    "Prefabs/HeavyObject", 
    parent: transform,
    startsInactive: true
);

// Setup the object while inactive
obj.GetComponent<Enemy>().Initialize(settings);

// Activate when ready
obj.SetActive(true);
```

### 3. Delayed Unload

```csharp
// Load a particle effect
GameObject explosion = await AddressableReferenceLoader.Load(
    "VFX/Explosion",
    transform
);

// Auto-unload after 3 seconds
AddressableReferenceLoader.Unload(explosion, delaySeconds: 3f);
```

### 4. Bulk Unloading

```csharp
public class LevelManager : MonoBehaviour
{
    void OnLevelEnd()
    {
        // Unload all Addressable instances at once
        // Perfect for scene transitions
        AddressableReferenceLoader.UnloadAllAssets();
    }
}
```

## 📖 Detailed Usage

### Load Method

```csharp
public static async UniTask<GameObject> Load(
    string key,                    // Addressable key or address
    Transform parent,              // Parent transform (can be null)
    bool startsInactive = false    // Start inactive?
)
```

**Returns:** The instantiated GameObject, or null if loading failed

**Features:**
- Validates key is not null/empty
- Tracks instance automatically
- Adds leak detection component
- Comprehensive error logging
- Thread-safe tracking

**Example:**
```csharp
// Load with parent
GameObject obj = await AddressableReferenceLoader.Load("MyPrefab", transform);

// Load at root level
GameObject obj = await AddressableReferenceLoader.Load("MyPrefab", null);

// Load inactive
GameObject obj = await AddressableReferenceLoader.Load("MyPrefab", transform, true);
```

### Unload Method (Immediate)

```csharp
public static void Unload(GameObject instance)
```

**Features:**
- Marks as unloaded (prevents leak warnings)
- Disposes all IDisposable components
- Releases Addressable handle
- Removes from tracking
- Safe to call multiple times

**Example:**
```csharp
GameObject enemy = await AddressableReferenceLoader.Load("Enemy", transform);

// When done:
AddressableReferenceLoader.Unload(enemy);
```

### Unload Method (Delayed)

```csharp
public static async UniTaskVoid Unload(GameObject instance, float delaySeconds)
```

**Features:**
- Marks as unloaded immediately (no leak warning)
- Waits for specified delay
- Auto-cancels if object destroyed
- Perfect for timed effects

**Example:**
```csharp
// Particle effect that lasts 2 seconds
GameObject particles = await AddressableReferenceLoader.Load("Particles", transform);
AddressableReferenceLoader.Unload(particles, 2f);
```

### UnloadAllAssets Method

```csharp
public static void UnloadAllAssets()
```

**Features:**
- Releases all cached handles
- Unloads all tracked instances
- Thread-safe bulk operation
- Logs count of released assets

**Use Cases:**
- Scene transitions
- Level ending
- Memory pressure
- Application shutdown

**Example:**
```csharp
void OnDestroy()
{
    // Clean up everything when manager is destroyed
    AddressableReferenceLoader.UnloadAllAssets();
}
```

## 🔍 Memory Leak Detection

The system automatically detects and warns about memory leaks:

### How It Works

1. Every loaded object gets an `AddressablesInstanceTracker` component
2. If object is destroyed without calling `Unload()`, tracker detects it
3. Warning is logged with diagnostic info
4. System attempts automatic cleanup
5. In editor, stack trace shows where object was created

### Example Leak Warning

```
[MEMORY LEAK DETECTED] Addressable instance 'Enemy(Clone)' (Key: Enemies/Goblin) 
was destroyed without being properly unloaded via AddressableReferenceLoader.Unload().
Lifetime: 5.32s
Forcing unload now to prevent memory leak.
Creation stack trace:
  at EnemySpawner.SpawnEnemy() in EnemySpawner.cs:line 42
  at GameManager.StartWave() in GameManager.cs:line 125
```

### Preventing Leaks

```csharp
// ❌ BAD - Will leak
GameObject obj = await AddressableReferenceLoader.Load("Prefab", transform);
Destroy(obj); // Tracker will catch this and warn

// ✅ GOOD - Proper cleanup
GameObject obj = await AddressableReferenceLoader.Load("Prefab", transform);
AddressableReferenceLoader.Unload(obj);

// ✅ ALSO GOOD - Delayed cleanup
GameObject obj = await AddressableReferenceLoader.Load("Prefab", transform);
AddressableReferenceLoader.Unload(obj, 3f);
```

## 🎮 Real-World Examples

### Example 1: Bullet System with Pooling

```csharp
using TakoBoyStudios.Core;
using Cysharp.Threading.Tasks;

public class Gun : MonoBehaviour
{
    [SerializeField] private string bulletKey = "Projectiles/Bullet";
    [SerializeField] private float bulletLifetime = 5f;
    
    public async UniTask Fire()
    {
        // Load bullet
        GameObject bullet = await AddressableReferenceLoader.Load(
            bulletKey,
            transform
        );
        
        if (bullet != null)
        {
            // Setup bullet
            bullet.transform.position = muzzle.position;
            bullet.transform.rotation = muzzle.rotation;
            
            var rb = bullet.GetComponent<Rigidbody>();
            rb.velocity = muzzle.forward * 20f;
            
            // Auto-destroy after lifetime
            AddressableReferenceLoader.Unload(bullet, bulletLifetime);
        }
    }
}
```

### Example 2: Enemy Spawner

```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private string[] enemyKeys;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    public async UniTask SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Random enemy type
            string key = enemyKeys[Random.Range(0, enemyKeys.Length)];
            
            // Load enemy
            GameObject enemy = await AddressableReferenceLoader.Load(
                key,
                transform
            );
            
            if (enemy != null)
            {
                enemy.transform.position = GetSpawnPosition();
                activeEnemies.Add(enemy);
                
                // Subscribe to death
                enemy.GetComponent<Enemy>().OnDeath += () => OnEnemyDied(enemy);
            }
        }
    }
    
    void OnEnemyDied(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
        
        // Unload after death animation
        AddressableReferenceLoader.Unload(enemy, 2f);
    }
    
    void OnDestroy()
    {
        // Clean up all enemies
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                AddressableReferenceLoader.Unload(enemy);
        }
        activeEnemies.Clear();
    }
}
```

### Example 3: Particle Effect Manager

```csharp
public class VFXManager : MonoBehaviour
{
    public async UniTask PlayEffect(string effectKey, Vector3 position, float duration)
    {
        GameObject vfx = await AddressableReferenceLoader.Load(
            effectKey,
            transform
        );
        
        if (vfx != null)
        {
            vfx.transform.position = position;
            
            // Auto-cleanup after effect duration
            AddressableReferenceLoader.Unload(vfx, duration);
        }
    }
    
    public async UniTask PlayExplosion(Vector3 position)
    {
        await PlayEffect("VFX/Explosion", position, 3f);
    }
    
    public async UniTask PlayHitEffect(Vector3 position)
    {
        await PlayEffect("VFX/Hit", position, 1f);
    }
}
```

### Example 4: Level Manager with Cleanup

```csharp
public class LevelManager : MonoBehaviour
{
    private List<GameObject> levelObjects = new List<GameObject>();
    
    public async UniTask LoadLevel(string levelKey)
    {
        // Load level prefab
        GameObject level = await AddressableReferenceLoader.Load(
            levelKey,
            transform
        );
        
        if (level != null)
        {
            levelObjects.Add(level);
        }
        
        // Load props
        await LoadLevelProps(level);
    }
    
    async UniTask LoadLevelProps(GameObject level)
    {
        Transform propsParent = level.transform.Find("Props");
        
        foreach (var propInfo in GetPropList())
        {
            GameObject prop = await AddressableReferenceLoader.Load(
                propInfo.key,
                propsParent
            );
            
            if (prop != null)
            {
                prop.transform.localPosition = propInfo.position;
                levelObjects.Add(prop);
            }
        }
    }
    
    public void UnloadLevel()
    {
        // Unload all level objects
        foreach (var obj in levelObjects)
        {
            if (obj != null)
                AddressableReferenceLoader.Unload(obj);
        }
        
        levelObjects.Clear();
    }
    
    void OnDestroy()
    {
        // Nuclear option - unload everything
        AddressableReferenceLoader.UnloadAllAssets();
    }
}
```

### Example 5: Asset Preloading

```csharp
public class AssetPreloader : MonoBehaviour
{
    [SerializeField] private string[] criticalAssets;
    [SerializeField] private Transform preloadContainer;
    
    private List<GameObject> preloadedAssets = new List<GameObject>();
    
    async UniTask Start()
    {
        await PreloadCriticalAssets();
    }
    
    async UniTask PreloadCriticalAssets()
    {
        Debug.Log("Preloading critical assets...");
        
        foreach (string key in criticalAssets)
        {
            // Load inactive so they're ready but not visible
            GameObject asset = await AddressableReferenceLoader.Load(
                key,
                preloadContainer,
                startsInactive: true
            );
            
            if (asset != null)
            {
                preloadedAssets.Add(asset);
                Debug.Log($"Preloaded: {key}");
            }
        }
        
        Debug.Log($"Preloading complete. {preloadedAssets.Count} assets ready.");
    }
    
    void OnDestroy()
    {
        // Clean up preloaded assets
        foreach (var asset in preloadedAssets)
        {
            if (asset != null)
                AddressableReferenceLoader.Unload(asset);
        }
        preloadedAssets.Clear();
    }
}
```

## 🔧 Advanced Usage

### IDisposable Component Support

The unload system automatically disposes components implementing IDisposable:

```csharp
public class PooledEnemy : MonoBehaviour, IDisposable
{
    private Timer timer;
    private Subscription subscription;
    
    public void Dispose()
    {
        // Called automatically when unloaded
        timer?.Dispose();
        subscription?.Unsubscribe();
    }
}

// When unloaded, Dispose() is called automatically
AddressableReferenceLoader.Unload(enemy);
```

### Diagnostics & Debugging

```csharp
// Get current instance count
int count = AddressableReferenceLoader.InstanceCount();
Debug.Log($"Currently tracking {count} instances");

// Try get count (thread-safe)
if (AddressableReferenceLoader.TryGetInstanceCount(out int safeCount))
{
    Debug.Log($"Safe count: {safeCount}");
}

// Show all loaded references
AddressableReferenceLoader.ShowAllReferences();

// Get detailed diagnostics
string diagnostics = AddressableReferenceLoader.GetDiagnostics();
Debug.Log(diagnostics);
```

### Manual Tracker Access

In rare cases, you may want to manually access the tracker:

```csharp
GameObject obj = await AddressableReferenceLoader.Load("Prefab", transform);

if (obj.TryGetComponent<AddressablesInstanceTracker>(out var tracker))
{
    // Get lifetime
    float lifetime = tracker.GetLifetime();
    
    // Check if unloaded
    bool unloaded = tracker.IsUnloaded();
    
    // Get diagnostics
    string info = tracker.GetDiagnostics();
    
#if UNITY_EDITOR
    // Get creation stack trace (editor only)
    string stackTrace = tracker.GetCreationStackTrace();
#endif
}
```

### Error Handling

```csharp
async UniTask LoadWithErrorHandling()
{
    try
    {
        GameObject obj = await AddressableReferenceLoader.Load("MayNotExist", transform);
        
        if (obj == null)
        {
            Debug.LogWarning("Failed to load asset");
            return;
        }
        
        // Use object
    }
    catch (Exception ex)
    {
        Debug.LogError($"Exception loading asset: {ex.Message}");
    }
}
```

## 💡 Best Practices

### 1. Always Unload When Done
```csharp
// DON'T - Memory leak
GameObject obj = await AddressableReferenceLoader.Load("Key", transform);
// Forgot to unload

// DO - Proper cleanup
GameObject obj = await AddressableReferenceLoader.Load("Key", transform);
AddressableReferenceLoader.Unload(obj);
```

### 2. Use Delayed Unload for Timed Effects
```csharp
// DON'T - Manual timing
GameObject vfx = await AddressableReferenceLoader.Load("VFX", transform);
await UniTask.Delay(TimeSpan.FromSeconds(3f));
AddressableReferenceLoader.Unload(vfx);

// DO - Built-in delayed unload
GameObject vfx = await AddressableReferenceLoader.Load("VFX", transform);
AddressableReferenceLoader.Unload(vfx, 3f);
```

### 3. Bulk Unload on Scene Transitions
```csharp
public class SceneTransition : MonoBehaviour
{
    async UniTask LoadNextScene()
    {
        // Clean up all Addressables before transition
        AddressableReferenceLoader.UnloadAllAssets();
        
        // Now safe to load next scene
        await SceneManager.LoadSceneAsync("NextScene");
    }
}
```

### 4. Check for Null After Loading
```csharp
GameObject obj = await AddressableReferenceLoader.Load("Key", transform);

if (obj == null)
{
    Debug.LogError("Failed to load");
    return;
}

// Safe to use obj
```

### 5. Track Your Instances
```csharp
public class MyManager : MonoBehaviour
{
    private List<GameObject> managedObjects = new List<GameObject>();
    
    async UniTask LoadSomething()
    {
        GameObject obj = await AddressableReferenceLoader.Load("Key", transform);
        if (obj != null)
            managedObjects.Add(obj);
    }
    
    void OnDestroy()
    {
        // Clean up tracked objects
        foreach (var obj in managedObjects)
        {
            if (obj != null)
                AddressableReferenceLoader.Unload(obj);
        }
        managedObjects.Clear();
    }
}
```

### 6. Use startsInactive for Heavy Objects
```csharp
// Load heavy object inactive
GameObject heavy = await AddressableReferenceLoader.Load(
    "HeavyObject",
    transform,
    startsInactive: true
);

// Setup while inactive (better performance)
heavy.GetComponent<HeavyComponent>().Initialize();

// Activate when ready
heavy.SetActive(true);
```

## 🐛 Troubleshooting

### "Key is null or empty" Error
**Problem:** Passing invalid key to Load()  
**Solution:** Ensure key is set and matches Addressables catalog

### "Invalid handle returned" Error
**Problem:** Addressable key doesn't exist  
**Solution:** Check Addressables window, verify key is correct

### "Null result from instantiating" Error
**Problem:** Key exists but prefab is empty/invalid  
**Solution:** Check the prefab asset in Addressables

### Memory Leak Warnings
**Problem:** Objects destroyed without calling Unload()  
**Solution:** Always call `Unload()` or use delayed unload

### Objects Not Unloading
**Problem:** Forgot to call Unload()  
**Solution:** Check OnDestroy() methods, use UnloadAllAssets() on cleanup

### Double Unload Warnings
**Problem:** Calling Unload() multiple times  
**Solution:** Track unload state, or ignore warning (it's safe)

## ⚡ Performance Tips

1. **Preload critical assets** - Load frequently-used assets at startup
2. **Use delayed unload** - Avoids immediate cleanup overhead
3. **Bulk unload on transitions** - More efficient than individual unloads
4. **Load inactive for heavy objects** - Setup before activation
5. **Monitor instance count** - Use diagnostics to track usage

## 🔄 Integration with Other Systems

### With PoolManager

```csharp
// If you need true pooling (reuse), use PoolManager
// If you just need lifecycle management, use AddressableReferenceLoader

// Addressables for loading, Pool for reuse
public async UniTask InitializeEnemyPool()
{
    // Load prefab once via Addressables
    GameObject prefab = await AddressableReferenceLoader.Load(
        "Enemy",
        null,
        startsInactive: true
    );
    
    // Create pool with that prefab
    PoolManager.Instance.CreatePool("Enemy", prefab, 10);
}
```

### With FSM

```csharp
public class GameStateMachine : MonoBehaviour
{
    private Fsm<GameState> fsm;
    
    void Start()
    {
        fsm = new Fsm<GameState>();
        fsm.AddState(GameState.Menu, MenuState);
        fsm.AddState(GameState.Playing, PlayingState);
        fsm.AddState(GameState.GameOver, GameOverState);
        
        fsm.OnStateExited += OnStateExited;
        fsm.ForceState(GameState.Menu);
    }
    
    void OnStateExited(GameState state)
    {
        // Clean up Addressables when leaving state
        if (state == GameState.Playing)
        {
            AddressableReferenceLoader.UnloadAllAssets();
        }
    }
}
```

## 📊 Dependencies

- **Unity Addressables** - Required
- **UniTask** (Cysharp.Threading.Tasks) - Required for async operations
- **TakoBoyStudios.Core namespace** - For consistency

## 📝 Summary

The Addressable Reference Loader provides:
- ✅ **Safe loading** - Automatic tracking and leak detection
- ✅ **Easy cleanup** - Simple Unload() API
- ✅ **Memory safety** - Catches and warns about leaks
- ✅ **Production-ready** - Comprehensive error handling
- ✅ **Developer-friendly** - Clear errors with stack traces
- ✅ **Performance** - Thread-safe, optimized operations

Perfect for any Unity project using Addressables! Never worry about memory leaks again! 🎉
