# TakoBoy Studios Core

Production-grade Unity utilities and systems for game development.

## Installation

### Via Package Manager (Recommended)

1. Open Unity Package Manager (Window > Package Manager)
2. Click the '+' button in the top left
3. Select "Add package from git URL"
4. Enter the repository URL or local path

### Manual Installation

Clone or download this repository into your project's `Packages` folder.

## Requirements

- **Unity 2021.3** or later
- **Addressables** 1.19.19+ (dependency)
- **UniTask** (optional, for async pool operations)

## Package Contents

### Extension Methods (9 files)

Production-ready extension methods for Unity types:

- **TransformExtensions.cs** - Transform manipulation, hierarchy operations, and searching
- **Vector3Extensions.cs** - 3D vector operations, rotations, and utilities
- **Vector2Extensions.cs** - 2D vector operations, rotations, and utilities
- **ColorExtensions.cs** - Color manipulation, conversion, and utilities
- **ArrayExtensions.cs** - Array utilities and operations
- **FloatExtensions.cs** - Float comparison and utility methods
- **IntExtensions.cs** - Integer utilities and operations
- **RectExtensions.cs** - Rect manipulation and utilities
- **LayerMaskExtensions.cs** - LayerMask utilities and operations

### Weighted Selection System (3 files)

Feature-complete weighted random selection system:

- **ISelectable.cs** - Interface for selectable items
- **Selector.cs** - Main selection logic with bonus system
- **Selector.Internal.cs** - Internal implementation details

**Features:**
- Single/multiple selection with weights
- Bonus system (adjust weights toward mean/median)
- Seeded random for reproducible results
- Selection history tracking
- Probability calculation
- Builder pattern API

**Perfect for:** Loot systems, enemy spawns, reward wheels, randomized content

### Object Pooling System (3 files)

Unity ObjectPool wrapper with convenience features:

- **IPoolable.cs** - Poolable object interface with lifecycle callbacks
- **PoolManager.cs** - Singleton pool manager
- **PoolExtensions.cs** - Extension methods for easy pooling

**Features:**
- Wraps Unity's ObjectPool<T> for performance
- Named pool management
- Async Addressables support (requires UniTask)
- Pre-warming and auto-grow
- Lifecycle callbacks (OnCreated, OnAcquired, OnReleased, OnDestroyed)
- Statistics tracking
- Extension methods for easy use

**Perfect for:** Bullets, enemies, particles, UI elements, frequently spawned objects

### Finite State Machine (2 files)

Lightweight FSM with enum inheritance support:

- **Fsm.cs** - Int-based FSM (supports enum inheritance)
- **FsmGeneric.cs** - Generic enum wrapper for type safety

**Features:**
- Integer-based (enum inheritance pattern support)
- Generic wrapper for type safety
- Update + FixedUpdate support
- Queued and immediate state changes
- State change events
- State history tracking
- Time in state tracking
- Try methods (non-throwing)
- Comprehensive validation

**Perfect for:** Player controllers, enemy AI, animation systems, game state management

### Addressables Helpers (2 files)

Utilities for working with Unity Addressables:

- **AddressableInstanceTracker.cs** - Track and manage Addressable instances
- **AddressableReferenceLoader.cs** - Helper for loading Addressable references

## Quick Start

### Basic Usage

```csharp
using TakoBoyStudios.Core;

// Extension methods
transform.SetPositionX(5f);
Vector3 randomPoint = bounds.GetRandomPoint();
Color brightColor = myColor.WithAlpha(0.5f);

// Weighted selection
var selector = new Selector<MyItem>();
selector.Add(item1, 10); // weight of 10
selector.Add(item2, 5);  // weight of 5
MyItem selected = selector.SelectOne();

// Object pooling
PoolManager.Instance.CreatePool("Bullet", bulletPrefab, 20);
GameObject bullet = PoolManager.Instance.Get("Bullet", position, rotation);
bullet.ReturnToPoolAfter(3f);

// State machine
var fsm = new Fsm<PlayerState>();
fsm.AddState(PlayerState.Idle, IdleUpdate);
fsm.AddState(PlayerState.Run, RunUpdate);
fsm.ForceState(PlayerState.Idle);
```

### Complete Example

```csharp
using TakoBoyStudios.Core;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Fsm<PlayerState> stateMachine;
    [SerializeField] private GameObject bulletPrefab;

    void Start()
    {
        // Setup FSM
        stateMachine = new Fsm<PlayerState>();
        stateMachine.AddState(PlayerState.Idle, IdleState);
        stateMachine.AddState(PlayerState.Run, RunState);
        stateMachine.ForceState(PlayerState.Idle);

        // Setup pool
        PoolManager.Instance.CreatePool("PlayerBullet", bulletPrefab, 20);
    }

    void Update()
    {
        stateMachine.Tick(Time.deltaTime);

        if (Input.GetButtonDown("Fire"))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        GameObject bullet = PoolManager.Instance.Get(
            "PlayerBullet",
            transform.position + transform.forward,
            transform.rotation
        );

        bullet.ReturnToPoolAfter(3f);
    }

    void IdleState(Fsm.StateStep step, float deltaTime)
    {
        // State logic here
        if (Input.GetAxis("Horizontal") != 0)
        {
            stateMachine.ChangeState(PlayerState.Run);
        }
    }

    void RunState(Fsm.StateStep step, float deltaTime)
    {
        // Run state logic
    }

    void OnDestroy()
    {
        stateMachine?.Dispose();
    }
}

public enum PlayerState { Idle, Run, Jump, Attack }
```

## Design Principles

All components follow these principles:

- **Production Ready** - Comprehensive error handling and validation
- **Well Documented** - XML documentation on all public APIs
- **Performance Optimized** - Efficient algorithms, minimal allocations
- **Unity Best Practices** - Follows Unity conventions and patterns
- **Namespace Consistency** - All under `TakoBoyStudios.Core`

## Performance Characteristics

- **Extension Methods**: Zero allocation, O(1) or O(n) operations
- **Selector**: O(log n) selection via binary search
- **PoolManager**: O(1) Get/Release, zero allocation after warmup
- **Fsm**: O(1) state changes and updates, minimal memory footprint

## Assembly Definition

The package includes `TakoBoyStudios.Core.asmdef` with:
- References to Unity.Addressables and Unity.ResourceManager
- Optional UniTask support via version defines
- Auto-referenced for easy use

## Dependencies

**Required:**
- Unity.Addressables (1.19.19+)

**Optional:**
- UniTask (com.cysharp.unitask) - For async pooling operations

## License

[Add your license here]

## Support

For issues, questions, or contributions, please visit the repository.
