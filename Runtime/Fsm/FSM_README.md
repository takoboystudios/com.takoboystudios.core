# Fsm - Finite State Machine

A lightweight, flexible finite state machine supporting both integer-based and generic enum-based states. Perfect for game AI, player controllers, animation systems, and any state-driven logic.

## 📦 Files

- **Fsm.cs** - Int-based FSM (supports enum inheritance patterns)
- **FsmGeneric.cs** - Generic enum wrapper for type-safe API

## ✨ Features

### Core Features
- ✅ **Integer-based states** - Supports enum inheritance patterns
- ✅ **Generic wrapper** - Type-safe enum API
- ✅ **Update + FixedUpdate** - Separate physics updates
- ✅ **Queued state changes** - Safe, predictable transitions
- ✅ **Immediate state forcing** - When you need instant changes
- ✅ **Lightweight** - Minimal overhead, fast performance

### Advanced Features
- ✅ **Events** - OnStateChanged, OnStateEntered, OnStateExited
- ✅ **State history** - Track last N state changes
- ✅ **Time tracking** - Know how long in current state
- ✅ **Try methods** - Non-throwing alternatives
- ✅ **State validation** - Prevent invalid states
- ✅ **Comprehensive logging** - Debug state behavior

## 🎯 Enum Inheritance Pattern

The int-based FSM supports a powerful enum inheritance pattern for extending base states:

```csharp
// Base entity states
public enum EntityState 
{ 
    Idle = 0,
    Dead = 1,
    Custom = 100  // Reserve space for derived enums
}

// Character-specific states extend from Custom
public enum CharacterState 
{ 
    Jump = EntityState.Custom,  // 100
    Roll,                       // 101
    Shoot                       // 102
}

// Boss-specific states also extend from Custom
public enum BossState
{
    Roar = EntityState.Custom,  // 100
    Summon,                     // 101
    Enrage                      // 102
}
```

This allows you to:
- Share common states across entity types
- Extend with type-specific states
- Use polymorphism in your state logic

## 🚀 Quick Start

### Option 1: Generic Enum FSM (Recommended for most cases)

```csharp
using TakoBoyStudios.Core;

public enum PlayerState { Idle, Run, Jump, Attack }

public class Player : MonoBehaviour
{
    private Fsm<PlayerState> fsm;
    
    void Start()
    {
        fsm = new Fsm<PlayerState>();
        
        // Add states
        fsm.AddState(PlayerState.Idle, IdleState);
        fsm.AddState(PlayerState.Run, RunState);
        fsm.AddState(PlayerState.Jump, JumpState);
        
        // Subscribe to events
        fsm.OnStateChanged += OnPlayerStateChanged;
        
        // Start in Idle
        fsm.ForceState(PlayerState.Idle);
    }
    
    void Update()
    {
        fsm.Tick(Time.deltaTime);
        
        // Check for state transitions
        if (Input.GetKeyDown(KeyCode.Space) && fsm.CurrentState == PlayerState.Idle)
        {
            fsm.ChangeState(PlayerState.Jump);
        }
    }
    
    void FixedUpdate()
    {
        fsm.PhysicsTick(Time.fixedDeltaTime);
    }
    
    void IdleState(Fsm.StateStep step, float deltaTime)
    {
        switch (step)
        {
            case Fsm.StateStep.Enter:
                Debug.Log("Entered Idle");
                break;
            case Fsm.StateStep.Update:
                // Idle logic
                break;
            case Fsm.StateStep.Exit:
                Debug.Log("Exiting Idle");
                break;
        }
    }
    
    void RunState(Fsm.StateStep step, float deltaTime)
    {
        // Run state logic
    }
    
    void JumpState(Fsm.StateStep step, float deltaTime)
    {
        // Jump state logic
    }
    
    void OnPlayerStateChanged(PlayerState from, PlayerState to)
    {
        Debug.Log($"Player: {from} -> {to}");
    }
    
    void OnDestroy()
    {
        fsm?.Dispose();
    }
}
```

### Option 2: Int-Based FSM (For enum inheritance)

```csharp
using TakoBoyStudios.Core;

public enum EntityState { Idle, Dead, Custom = 100 }
public enum CharacterState { Jump = EntityState.Custom, Roll, Shoot }

public class Character : MonoBehaviour
{
    private Fsm fsm;
    
    void Start()
    {
        fsm = new Fsm();
        
        // Add base entity states
        fsm.AddState((int)EntityState.Idle, IdleState);
        fsm.AddState((int)EntityState.Dead, DeadState);
        
        // Add character-specific states
        fsm.AddState((int)CharacterState.Jump, JumpState);
        fsm.AddState((int)CharacterState.Roll, RollState);
        
        // Subscribe to events
        fsm.OnStateChanged += (from, to) => 
            Debug.Log($"State: {fsm.GetStateName(from)} -> {fsm.GetStateName(to)}");
        
        fsm.ForceState((int)EntityState.Idle);
    }
    
    void Update()
    {
        fsm.Tick(Time.deltaTime);
    }
    
    void IdleState(Fsm.StateStep step, float deltaTime) { /* ... */ }
    void JumpState(Fsm.StateStep step, float deltaTime) { /* ... */ }
    void RollState(Fsm.StateStep step, float deltaTime) { /* ... */ }
    void DeadState(Fsm.StateStep step, float deltaTime) { /* ... */ }
}
```

## 📖 Detailed Usage

### State Functions

States are defined as methods matching the `Fsm.State` delegate:

```csharp
void MyState(Fsm.StateStep step, float deltaTime)
{
    switch (step)
    {
        case Fsm.StateStep.Enter:
            // Called once when entering the state
            Debug.Log("Entering MyState");
            animator.SetTrigger("Enter");
            break;
            
        case Fsm.StateStep.Update:
            // Called every frame while in this state
            transform.position += Vector3.forward * speed * deltaTime;
            break;
            
        case Fsm.StateStep.FixedUpdate:
            // Called every physics update while in this state
            rb.AddForce(Vector3.up * jumpForce);
            break;
            
        case Fsm.StateStep.Exit:
            // Called once when exiting the state
            Debug.Log("Exiting MyState");
            break;
    }
}
```

### State Transitions

#### Queued (Safe - happens on next Tick)
```csharp
// Queue a state change (applied on next Tick)
fsm.ChangeState(PlayerState.Run);

// Try to queue (returns false if invalid)
if (fsm.TryChangeState(PlayerState.Jump))
{
    Debug.Log("Jump queued!");
}

// Cancel queued change
fsm.CancelPendingChange();
```

#### Immediate (Use sparingly)
```csharp
// Force immediate state change
fsm.ForceState(PlayerState.Dead);

// Try to force immediately
if (fsm.TryForceState(PlayerState.Dead))
{
    Debug.Log("Died immediately!");
}
```

### Events

```csharp
// State changed event (from, to)
fsm.OnStateChanged += (from, to) =>
{
    Debug.Log($"Changed from {from} to {to}");
    PlayTransitionSound();
};

// State entered event
fsm.OnStateEntered += (state) =>
{
    Debug.Log($"Entered {state}");
    UpdateUI();
};

// State exited event
fsm.OnStateExited += (state) =>
{
    Debug.Log($"Exited {state}");
    CleanupState(state);
};
```

### State Queries

```csharp
// Current state info
PlayerState current = fsm.CurrentState;
string currentName = fsm.CurrentStateName;
float timeInState = fsm.TimeInCurrentState;

// Check if changing
if (fsm.IsChangingStates)
{
    PlayerState next = fsm.NextState;
}

// Previous state
PlayerState previous = fsm.PreviousState;

// State history
PlayerState[] history = fsm.GetStateHistory();
PlayerState[] last3 = fsm.GetStateHistory(3);

// Transition count
int transitions = fsm.TransitionCount;

// Validation
if (fsm.IsStateValid(PlayerState.Jump))
{
    // State is registered
}

// Get all states
PlayerState[] allStates = fsm.GetAllStates();
```

## 🎮 Real-World Examples

### Example 1: Player Controller

```csharp
public class PlayerController : MonoBehaviour
{
    public enum State { Idle, Walk, Run, Jump, Fall, Land, Attack }
    
    private Fsm<State> fsm;
    private CharacterController controller;
    private Vector3 velocity;
    
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;
    
    void Start()
    {
        fsm = new Fsm<State>();
        controller = GetComponent<CharacterController>();
        
        // Setup states
        fsm.AddState(State.Idle, IdleState);
        fsm.AddState(State.Walk, WalkState);
        fsm.AddState(State.Run, RunState);
        fsm.AddState(State.Jump, JumpState);
        fsm.AddState(State.Fall, FallState);
        fsm.AddState(State.Land, LandState);
        fsm.AddState(State.Attack, AttackState);
        
        // Events
        fsm.OnStateEntered += OnStateEntered;
        
        fsm.ForceState(State.Idle);
    }
    
    void Update()
    {
        fsm.Tick(Time.deltaTime);
    }
    
    void IdleState(Fsm.StateStep step, float deltaTime)
    {
        if (step != Fsm.StateStep.Update) return;
        
        Vector2 input = GetMovementInput();
        
        if (input.magnitude > 0.1f)
        {
            fsm.ChangeState(Input.GetKey(KeyCode.LeftShift) ? State.Run : State.Walk);
        }
        else if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            fsm.ChangeState(State.Jump);
        }
        else if (Input.GetButtonDown("Fire1"))
        {
            fsm.ChangeState(State.Attack);
        }
    }
    
    void WalkState(Fsm.StateStep step, float deltaTime)
    {
        if (step != Fsm.StateStep.Update) return;
        
        Vector2 input = GetMovementInput();
        
        if (input.magnitude < 0.1f)
        {
            fsm.ChangeState(State.Idle);
        }
        else
        {
            Move(input, walkSpeed, deltaTime);
            
            if (Input.GetKey(KeyCode.LeftShift))
                fsm.ChangeState(State.Run);
        }
        
        CheckJumpAndFall();
    }
    
    void RunState(Fsm.StateStep step, float deltaTime)
    {
        if (step != Fsm.StateStep.Update) return;
        
        Vector2 input = GetMovementInput();
        
        if (input.magnitude < 0.1f)
        {
            fsm.ChangeState(State.Idle);
        }
        else
        {
            Move(input, runSpeed, deltaTime);
            
            if (!Input.GetKey(KeyCode.LeftShift))
                fsm.ChangeState(State.Walk);
        }
        
        CheckJumpAndFall();
    }
    
    void JumpState(Fsm.StateStep step, float deltaTime)
    {
        switch (step)
        {
            case Fsm.StateStep.Enter:
                velocity.y = jumpForce;
                break;
                
            case Fsm.StateStep.Update:
                velocity.y += gravity * deltaTime;
                controller.Move(velocity * deltaTime);
                
                if (velocity.y < 0)
                    fsm.ChangeState(State.Fall);
                break;
        }
    }
    
    void FallState(Fsm.StateStep step, float deltaTime)
    {
        if (step != Fsm.StateStep.Update) return;
        
        velocity.y += gravity * deltaTime;
        controller.Move(velocity * deltaTime);
        
        if (controller.isGrounded)
        {
            fsm.ChangeState(State.Land);
        }
    }
    
    void LandState(Fsm.StateStep step, float deltaTime)
    {
        switch (step)
        {
            case Fsm.StateStep.Enter:
                velocity.y = 0;
                // Play landing animation/sound
                break;
                
            case Fsm.StateStep.Update:
                // Short landing state, auto-transition after brief time
                if (fsm.TimeInCurrentState > 0.2f)
                {
                    fsm.ChangeState(State.Idle);
                }
                break;
        }
    }
    
    void AttackState(Fsm.StateStep step, float deltaTime)
    {
        switch (step)
        {
            case Fsm.StateStep.Enter:
                // Trigger attack animation
                break;
                
            case Fsm.StateStep.Update:
                // Attack duration
                if (fsm.TimeInCurrentState > 0.5f)
                {
                    fsm.ChangeState(State.Idle);
                }
                break;
        }
    }
    
    void OnStateEntered(State state)
    {
        Debug.Log($"Player entered: {state}");
        // Update animator parameters
        // Play sounds
        // Update UI
    }
    
    Vector2 GetMovementInput()
    {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
    
    void Move(Vector2 input, float speed, float deltaTime)
    {
        Vector3 move = new Vector3(input.x, 0, input.y) * speed;
        controller.Move(move * deltaTime);
    }
    
    void CheckJumpAndFall()
    {
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
            fsm.ChangeState(State.Jump);
        else if (!controller.isGrounded && velocity.y < 0)
            fsm.ChangeState(State.Fall);
    }
    
    void OnDestroy()
    {
        fsm?.Dispose();
    }
}
```

### Example 2: Enemy AI with Enum Inheritance

```csharp
// Base states all enemies share
public enum EntityState 
{ 
    Idle = 0,
    Dead = 1,
    Stunned = 2,
    Custom = 100 
}

// Melee enemy specific states
public enum MeleeEnemyState 
{ 
    Patrol = EntityState.Custom,
    Chase,
    MeleeAttack,
    Block
}

public class MeleeEnemy : MonoBehaviour
{
    private Fsm fsm;
    private Transform player;
    
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackRange = 2f;
    
    void Start()
    {
        fsm = new Fsm();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // Base states
        fsm.AddState((int)EntityState.Idle, BaseIdleState);
        fsm.AddState((int)EntityState.Dead, BaseDeadState);
        fsm.AddState((int)EntityState.Stunned, BaseStunnedState);
        
        // Melee-specific states
        fsm.AddState((int)MeleeEnemyState.Patrol, PatrolState);
        fsm.AddState((int)MeleeEnemyState.Chase, ChaseState);
        fsm.AddState((int)MeleeEnemyState.MeleeAttack, MeleeAttackState);
        fsm.AddState((int)MeleeEnemyState.Block, BlockState);
        
        fsm.OnStateChanged += OnAIStateChanged;
        
        fsm.ForceState((int)MeleeEnemyState.Patrol);
    }
    
    void Update()
    {
        fsm.Tick(Time.deltaTime);
    }
    
    // Base entity states (shared across all enemy types)
    void BaseIdleState(Fsm.StateStep step, float deltaTime)
    {
        // Common idle logic
    }
    
    void BaseDeadState(Fsm.StateStep step, float deltaTime)
    {
        if (step == Fsm.StateStep.Enter)
        {
            // Death logic - disable colliders, play death anim
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, 3f);
        }
    }
    
    void BaseStunnedState(Fsm.StateStep step, float deltaTime)
    {
        if (step == Fsm.StateStep.Enter)
        {
            // Play stunned effect
        }
        else if (step == Fsm.StateStep.Update)
        {
            if (fsm.TimeInCurrentState > 2f)
                fsm.ChangeState((int)MeleeEnemyState.Patrol);
        }
    }
    
    // Melee-specific states
    void PatrolState(Fsm.StateStep step, float deltaTime)
    {
        if (step != Fsm.StateStep.Update) return;
        
        // Patrol waypoints
        
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer < detectionRange)
        {
            fsm.ChangeState((int)MeleeEnemyState.Chase);
        }
    }
    
    void ChaseState(Fsm.StateStep step, float deltaTime)
    {
        if (step != Fsm.StateStep.Update) return;
        
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distToPlayer < attackRange)
        {
            fsm.ChangeState((int)MeleeEnemyState.MeleeAttack);
        }
        else if (distToPlayer > detectionRange * 1.5f)
        {
            fsm.ChangeState((int)MeleeEnemyState.Patrol);
        }
        else
        {
            // Move toward player
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * 3f * deltaTime;
        }
    }
    
    void MeleeAttackState(Fsm.StateStep step, float deltaTime)
    {
        switch (step)
        {
            case Fsm.StateStep.Enter:
                // Start attack animation
                break;
                
            case Fsm.StateStep.Update:
                if (fsm.TimeInCurrentState > 1f)
                {
                    // Attack finished
                    fsm.ChangeState((int)MeleeEnemyState.Chase);
                }
                break;
        }
    }
    
    void BlockState(Fsm.StateStep step, float deltaTime)
    {
        // Block logic
    }
    
    void OnAIStateChanged(int from, int to)
    {
        Debug.Log($"Enemy AI: {fsm.GetStateName(from)} -> {fsm.GetStateName(to)}");
    }
    
    public void TakeDamage(int damage)
    {
        // Can force state from external events
        if (Random.value < 0.3f)
            fsm.ForceState((int)EntityState.Stunned);
        
        // Check if dead
        if (health <= 0)
            fsm.ForceState((int)EntityState.Dead);
    }
    
    void OnDestroy()
    {
        fsm?.Dispose();
    }
}
```

### Example 3: Animation State Machine

```csharp
public class AnimationController : MonoBehaviour
{
    public enum AnimState 
    { 
        Idle, Walk, Run, Jump, Fall, 
        Attack1, Attack2, Attack3, 
        Hit, Death 
    }
    
    private Fsm<AnimState> fsm;
    private Animator animator;
    
    void Start()
    {
        fsm = new Fsm<AnimState>();
        animator = GetComponent<Animator>();
        
        // Add all animation states
        foreach (AnimState state in Enum.GetValues(typeof(AnimState)))
        {
            fsm.AddState(state, AnimationState);
        }
        
        // Use events to update animator
        fsm.OnStateEntered += (state) =>
        {
            animator.SetTrigger(state.ToString());
        };
        
        fsm.ForceState(AnimState.Idle);
    }
    
    void AnimationState(Fsm.StateStep step, float deltaTime)
    {
        // Common animation logic for all states
        // Or handle specific states differently
    }
    
    // Public API for other systems to trigger animations
    public void PlayAnimation(AnimState state)
    {
        if (fsm.IsStateValid(state))
        {
            fsm.ChangeState(state);
        }
    }
    
    public void PlayAnimationImmediate(AnimState state)
    {
        fsm.ForceState(state);
    }
}
```

## 🔧 Advanced Usage

### State History Debugging

```csharp
void Update()
{
    fsm.Tick(Time.deltaTime);
    
    // Debug last 5 states on key press
    if (Input.GetKeyDown(KeyCode.H))
    {
        var history = fsm.GetStateHistory(5);
        Debug.Log("Recent states: " + string.Join(" -> ", history));
    }
}
```

### Time-Based Auto-Transitions

```csharp
void TimedState(Fsm.StateStep step, float deltaTime)
{
    if (step == Fsm.StateStep.Update)
    {
        if (fsm.TimeInCurrentState > 3f)
        {
            fsm.ChangeState(NextState);
        }
    }
}
```

### Conditional State Validation

```csharp
public bool CanTransitionTo(PlayerState targetState)
{
    PlayerState current = fsm.CurrentState;
    
    // Define allowed transitions
    switch (current)
    {
        case PlayerState.Idle:
            return targetState == PlayerState.Walk || 
                   targetState == PlayerState.Jump;
                   
        case PlayerState.Walk:
            return targetState != PlayerState.Attack; // Can't attack while walking
            
        case PlayerState.Jump:
            return targetState == PlayerState.Fall; // Can only fall from jump
            
        default:
            return true;
    }
}

// Use before transitioning
if (CanTransitionTo(PlayerState.Attack))
{
    fsm.ChangeState(PlayerState.Attack);
}
```

## 💡 Best Practices

### 1. Always Dispose FSMs
```csharp
void OnDestroy()
{
    fsm?.Dispose();
}
```

### 2. Use Events for Side Effects
```csharp
// DON'T do this in state functions
void MyState(Fsm.StateStep step, float deltaTime)
{
    if (step == Fsm.StateStep.Enter)
    {
        UpdateUI(); // State shouldn't know about UI
    }
}

// DO this with events
fsm.OnStateEntered += (state) =>
{
    UpdateUI(); // Separate concerns
};
```

### 3. Prefer Queued Changes
```csharp
// GOOD - Queued (safe, predictable)
fsm.ChangeState(PlayerState.Run);

// USE SPARINGLY - Immediate (can cause issues)
fsm.ForceState(PlayerState.Dead);
```

### 4. Use Try Methods for Optional Transitions
```csharp
// Instead of checking validity manually
if (fsm.IsStateValid(PlayerState.Jump))
{
    fsm.ChangeState(PlayerState.Jump);
}

// Just use TryChangeState
if (fsm.TryChangeState(PlayerState.Jump))
{
    // Successfully queued
}
```

### 5. Leverage TimeInCurrentState
```csharp
void AttackState(Fsm.StateStep step, float deltaTime)
{
    if (step == Fsm.StateStep.Update)
    {
        // Phase 1: Wind-up
        if (fsm.TimeInCurrentState < 0.3f)
        {
            // Charge animation
        }
        // Phase 2: Hit
        else if (fsm.TimeInCurrentState < 0.5f)
        {
            // Deal damage
        }
        // Phase 3: Recovery
        else if (fsm.TimeInCurrentState < 0.8f)
        {
            // Recovery animation
        }
        else
        {
            fsm.ChangeState(PlayerState.Idle);
        }
    }
}
```

## 🐛 Troubleshooting

### "State not found" errors
**Problem:** Trying to transition to unregistered state  
**Solution:** Call `AddState()` before using the state

### States not updating
**Problem:** Forgot to call `Tick()`  
**Solution:** Call `fsm.Tick(Time.deltaTime)` in `Update()`

### Physics not working
**Problem:** Forgot to call `PhysicsTick()`  
**Solution:** Call `fsm.PhysicsTick(Time.fixedDeltaTime)` in `FixedUpdate()`

### Enum inheritance not working
**Problem:** Derived enum values overlap base enum values  
**Solution:** Start derived enums at a high number (e.g., 100)

```csharp
// BAD - Overlaps
public enum Base { A, B, C }           // 0, 1, 2
public enum Derived { D = Base.C }     // 2 - OVERLAPS Base.C!

// GOOD - No overlap
public enum Base { A, B, Custom = 100 }
public enum Derived { D = Base.Custom } // 100 - Safe!
```

## 📝 Summary

The FSM system provides:
- **Lightweight** int-based FSM for performance
- **Type-safe** generic wrapper for convenience
- **Enum inheritance** support for extensible state systems
- **Rich events** for loose coupling
- **State tracking** for debugging and analytics
- **Flexible API** with both queued and immediate transitions

Perfect for player controllers, AI, animation systems, game states, UI flows, and any state-driven logic!
