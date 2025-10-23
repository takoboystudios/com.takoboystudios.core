using System;
using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Generic enum-based finite state machine wrapper providing type-safe state management.
    /// Wraps the int-based Fsm internally for optimal performance.
    /// </summary>
    /// <typeparam name="TState">The enum type representing states</typeparam>
    /// <remarks>
    /// This wrapper provides a type-safe API over the int-based Fsm, making it easier
    /// to work with enums directly without manual casting.
    /// 
    /// The underlying int-based FSM still supports enum inheritance patterns:
    /// <code>
    /// public enum EntityState { Idle, Dead, Custom = 100 }
    /// public enum CharacterState { Jump = EntityState.Custom, Roll, Shoot }
    /// 
    /// var fsm = new Fsm&lt;CharacterState&gt;();
    /// fsm.AddState(CharacterState.Jump, JumpState);
    /// fsm.ChangeState(CharacterState.Roll);
    /// </code>
    /// </remarks>
    /// <example>
    /// <code>
    /// public enum PlayerState { Idle, Run, Jump, Attack }
    /// 
    /// var fsm = new Fsm&lt;PlayerState&gt;();
    /// fsm.AddState(PlayerState.Idle, IdleState);
    /// fsm.AddState(PlayerState.Run, RunState);
    /// fsm.OnStateChanged += (from, to) => Debug.Log($"{from} -> {to}");
    /// fsm.ChangeState(PlayerState.Run);
    /// fsm.Tick(Time.deltaTime);
    /// </code>
    /// </example>
    public class Fsm<TState> : IDisposable where TState : struct, Enum
    {
        #region Private Fields

        private readonly Fsm m_innerFsm;

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the state machine changes from one state to another.
        /// Parameters: (fromState, toState)
        /// </summary>
        public event Action<TState, TState> OnStateChanged;

        /// <summary>
        /// Invoked when entering a new state, after OnStateChanged.
        /// Parameter: (newState)
        /// </summary>
        public event Action<TState> OnStateEntered;

        /// <summary>
        /// Invoked when exiting the current state, before OnStateChanged.
        /// Parameter: (oldState)
        /// </summary>
        public event Action<TState> OnStateExited;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the previous state.
        /// </summary>
        public TState PreviousState => ToEnum(m_innerFsm.PreviousState);

        /// <summary>
        /// Gets the next queued state, or default if no change is pending.
        /// </summary>
        public TState NextState => ToEnum(m_innerFsm.NextState);

        /// <summary>
        /// Gets the current active state.
        /// </summary>
        public TState CurrentState => ToEnum(m_innerFsm.CurrentState);

        /// <summary>
        /// Gets the name of the previous state.
        /// </summary>
        public string PreviousStateName => m_innerFsm.PreviousStateName;

        /// <summary>
        /// Gets the name of the next queued state.
        /// </summary>
        public string NextStateName => m_innerFsm.NextStateName;

        /// <summary>
        /// Gets the name of the current state.
        /// </summary>
        public string CurrentStateName => m_innerFsm.CurrentStateName;

        /// <summary>
        /// Returns true if a state change is queued.
        /// </summary>
        public bool IsChangingStates => m_innerFsm.IsChangingStates;

        /// <summary>
        /// Gets the time (in seconds) spent in the current state.
        /// </summary>
        public float TimeInCurrentState => m_innerFsm.TimeInCurrentState;

        /// <summary>
        /// Gets the total number of state transitions that have occurred.
        /// </summary>
        public int TransitionCount => m_innerFsm.TransitionCount;

        /// <summary>
        /// Gets whether this FSM has been disposed.
        /// </summary>
        public bool IsDisposed => m_innerFsm.IsDisposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new generic FSM with default history size of 10.
        /// </summary>
        public Fsm() : this(10)
        {
        }

        /// <summary>
        /// Creates a new generic FSM with specified history size.
        /// </summary>
        /// <param name="historySize">Maximum number of previous states to track</param>
        public Fsm(int historySize)
        {
            m_innerFsm = new Fsm(historySize);

            // Wire up events
            m_innerFsm.OnStateChanged += OnInnerStateChanged;
            m_innerFsm.OnStateEntered += OnInnerStateEntered;
            m_innerFsm.OnStateExited += OnInnerStateExited;
        }

        #endregion

        #region State Management

        /// <summary>
        /// Adds a state with type-safe enum ID.
        /// </summary>
        /// <param name="state">The state enum value</param>
        /// <param name="definition">The state function</param>
        public void AddState(TState state, Fsm.State definition)
        {
            m_innerFsm.AddState(ToInt(state), definition);
        }

        /// <summary>
        /// Adds a state with a custom name for debugging.
        /// </summary>
        /// <param name="state">The state enum value</param>
        /// <param name="definition">The state function</param>
        /// <param name="customName">Custom name for this state</param>
        public void AddState(TState state, Fsm.State definition, string customName)
        {
            m_innerFsm.AddState(ToInt(state), definition, customName);
        }

        /// <summary>
        /// Removes a state from the FSM.
        /// </summary>
        /// <param name="state">The state to remove</param>
        /// <returns>True if the state was removed</returns>
        public bool RemoveState(TState state)
        {
            return m_innerFsm.RemoveState(ToInt(state));
        }

        /// <summary>
        /// Checks if a state is valid and registered.
        /// </summary>
        /// <param name="state">The state to check</param>
        /// <returns>True if the state is valid</returns>
        public bool IsStateValid(TState state)
        {
            return m_innerFsm.IsStateValid(ToInt(state));
        }

        /// <summary>
        /// Checks if the current state is valid.
        /// </summary>
        /// <returns>True if current state is valid</returns>
        public bool IsCurrentStateValid()
        {
            return m_innerFsm.IsCurrentStateValid();
        }

        /// <summary>
        /// Gets the name of a state.
        /// </summary>
        /// <param name="state">The state</param>
        /// <returns>The state name</returns>
        public string GetStateName(TState state)
        {
            return m_innerFsm.GetStateName(ToInt(state));
        }

        /// <summary>
        /// Gets all registered states.
        /// </summary>
        /// <returns>Array of state enum values</returns>
        public TState[] GetAllStates()
        {
            int[] intStates = m_innerFsm.GetAllStates();
            TState[] states = new TState[intStates.Length];

            for (int i = 0; i < intStates.Length; i++)
            {
                states[i] = ToEnum(intStates[i]);
            }

            return states;
        }

        #endregion

        #region State Transitions

        /// <summary>
        /// Queues a state change to occur on the next Tick.
        /// </summary>
        /// <param name="state">The target state</param>
        public void ChangeState(TState state)
        {
            m_innerFsm.ChangeState(ToInt(state));
        }

        /// <summary>
        /// Attempts to queue a state change without throwing exceptions.
        /// </summary>
        /// <param name="state">The target state</param>
        /// <returns>True if the state is valid and was queued</returns>
        public bool TryChangeState(TState state)
        {
            return m_innerFsm.TryChangeState(ToInt(state));
        }

        /// <summary>
        /// Immediately forces a state change without waiting for next Tick.
        /// </summary>
        /// <param name="state">The target state</param>
        /// <param name="deltaTime">Delta time to pass to Enter/Exit callbacks</param>
        public void ForceState(TState state, float deltaTime = 0f)
        {
            m_innerFsm.ForceState(ToInt(state), deltaTime);
        }

        /// <summary>
        /// Attempts to immediately force a state change without throwing exceptions.
        /// </summary>
        /// <param name="state">The target state</param>
        /// <param name="deltaTime">Delta time to pass to callbacks</param>
        /// <returns>True if the state change succeeded</returns>
        public bool TryForceState(TState state, float deltaTime = 0f)
        {
            return m_innerFsm.TryForceState(ToInt(state), deltaTime);
        }

        /// <summary>
        /// Cancels a pending state change.
        /// </summary>
        public void CancelPendingChange()
        {
            m_innerFsm.CancelPendingChange();
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Updates the state machine, processing queued state changes and calling Update on the current state.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Tick(float deltaTime)
        {
            m_innerFsm.Tick(deltaTime);
        }

        /// <summary>
        /// Updates the state machine for physics, calling FixedUpdate on the current state.
        /// </summary>
        /// <param name="fixedDeltaTime">Fixed delta time</param>
        public void PhysicsTick(float fixedDeltaTime)
        {
            m_innerFsm.PhysicsTick(fixedDeltaTime);
        }

        #endregion

        #region State History

        /// <summary>
        /// Gets the state history as an array (oldest to newest).
        /// </summary>
        /// <returns>Array of previous states</returns>
        public TState[] GetStateHistory()
        {
            int[] intHistory = m_innerFsm.GetStateHistory();
            TState[] history = new TState[intHistory.Length];

            for (int i = 0; i < intHistory.Length; i++)
            {
                history[i] = ToEnum(intHistory[i]);
            }

            return history;
        }

        /// <summary>
        /// Gets the last N states from history.
        /// </summary>
        /// <param name="count">Number of states to retrieve</param>
        /// <returns>Array of states (up to count)</returns>
        public TState[] GetStateHistory(int count)
        {
            int[] intHistory = m_innerFsm.GetStateHistory(count);
            TState[] history = new TState[intHistory.Length];

            for (int i = 0; i < intHistory.Length; i++)
            {
                history[i] = ToEnum(intHistory[i]);
            }

            return history;
        }

        /// <summary>
        /// Clears the state history.
        /// </summary>
        public void ClearHistory()
        {
            m_innerFsm.ClearHistory();
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Disposes the FSM and clears all states.
        /// </summary>
        public void Dispose()
        {
            // Unwire events
            m_innerFsm.OnStateChanged -= OnInnerStateChanged;
            m_innerFsm.OnStateEntered -= OnInnerStateEntered;
            m_innerFsm.OnStateExited -= OnInnerStateExited;

            // Clear our events
            OnStateChanged = null;
            OnStateEntered = null;
            OnStateExited = null;

            // Dispose inner FSM
            m_innerFsm.Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Converts enum to int for internal FSM.
        /// </summary>
        private int ToInt(TState state)
        {
            return Convert.ToInt32(state);
        }

        /// <summary>
        /// Converts int from internal FSM to enum.
        /// </summary>
        private TState ToEnum(int stateInt)
        {
            if (stateInt == Fsm.NO_STATE)
                return default(TState);

            return (TState)Enum.ToObject(typeof(TState), stateInt);
        }

        /// <summary>
        /// Event handler for inner FSM state changes.
        /// </summary>
        private void OnInnerStateChanged(int fromInt, int toInt)
        {
            try
            {
                TState from = ToEnum(fromInt);
                TState to = ToEnum(toInt);
                OnStateChanged?.Invoke(from, to);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Fsm<{typeof(TState).Name}>] Error in OnStateChanged event: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for inner FSM state enter.
        /// </summary>
        private void OnInnerStateEntered(int stateInt)
        {
            try
            {
                TState state = ToEnum(stateInt);
                OnStateEntered?.Invoke(state);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Fsm<{typeof(TState).Name}>] Error in OnStateEntered event: {ex.Message}");
            }
        }

        /// <summary>
        /// Event handler for inner FSM state exit.
        /// </summary>
        private void OnInnerStateExited(int stateInt)
        {
            try
            {
                TState state = ToEnum(stateInt);
                OnStateExited?.Invoke(state);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Fsm<{typeof(TState).Name}>] Error in OnStateExited event: {ex.Message}");
            }
        }

        #endregion
    }
}
