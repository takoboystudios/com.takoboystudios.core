using System;
using System.Collections.Generic;
using UnityEngine;

namespace TakoBoyStudios.Core
{
    /// <summary>
    /// Lightweight finite state machine using integer-based state identification.
    /// Supports enum inheritance patterns where derived enums extend base enum values.
    /// </summary>
    /// <remarks>
    /// This FSM uses integer state IDs, making it compatible with enum inheritance:
    /// <code>
    /// public enum EntityState { Idle, Dead, Custom = 100 }
    /// public enum CharacterState { Jump = EntityState.Custom, Roll, Shoot }
    /// </code>
    /// 
    /// Features:
    /// - Integer-based states (enum-compatible)
    /// - Update and FixedUpdate support
    /// - Queued state changes (applied on next Tick)
    /// - State change events
    /// - State history tracking
    /// - Time in state tracking
    /// - Try methods for safe operations
    /// </remarks>
    /// <example>
    /// <code>
    /// var fsm = new Fsm();
    /// fsm.AddState((int)EntityState.Idle, IdleState);
    /// fsm.AddState((int)CharacterState.Jump, JumpState);
    /// fsm.OnStateChanged += (from, to) => Debug.Log($"Changed: {from} -> {to}");
    /// fsm.ChangeState((int)CharacterState.Jump);
    /// fsm.Tick(Time.deltaTime);
    /// </code>
    /// </example>
    public class Fsm : IDisposable
    {
        #region Constants

        /// <summary>
        /// Sentinel value representing no state or invalid state.
        /// </summary>
        public const int NO_STATE = int.MaxValue;

        /// <summary>
        /// Name returned for invalid or unrecognized states.
        /// </summary>
        public const string INVALID_STATE_NAME = "INVALID";

        #endregion

        #region Enums

        /// <summary>
        /// Represents the different lifecycle steps of a state.
        /// </summary>
        public enum StateStep
        {
            /// <summary>
            /// Called once when entering a state.
            /// </summary>
            Enter,

            /// <summary>
            /// Called every frame while in a state.
            /// </summary>
            Update,

            /// <summary>
            /// Called every physics update while in a state.
            /// </summary>
            FixedUpdate,

            /// <summary>
            /// Called once when exiting a state.
            /// </summary>
            Exit,
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked when the state machine changes from one state to another.
        /// Parameters: (fromState, toState)
        /// </summary>
        public event Action<int, int> OnStateChanged;

        /// <summary>
        /// Invoked when entering a new state, after OnStateChanged.
        /// Parameter: (newState)
        /// </summary>
        public event Action<int> OnStateEntered;

        /// <summary>
        /// Invoked when exiting the current state, before OnStateChanged.
        /// Parameter: (oldState)
        /// </summary>
        public event Action<int> OnStateExited;

        #endregion

        #region Delegates

        /// <summary>
        /// Delegate type for state functions.
        /// </summary>
        /// <param name="step">The current lifecycle step</param>
        /// <param name="deltaTime">Time since last update</param>
        public delegate void State(StateStep step, float deltaTime);

        #endregion

        #region Private Fields

        private int m_previousState;
        private int m_nextState;
        private int m_currentState;
        private bool m_disposed;
        private float m_timeInCurrentState;
        private int m_transitionCount;

        private readonly Dictionary<int, State> m_stateMap;
        private readonly Dictionary<int, string> m_stateNames;
        private readonly Queue<int> m_stateHistory;
        private readonly int m_maxHistorySize;

        // Legacy support for index-based AddState
        private readonly List<State> m_states;
        private int m_nextAutoIndex;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the previous state ID.
        /// </summary>
        public int PreviousState => m_previousState;

        /// <summary>
        /// Gets the next queued state ID, or NO_STATE if no change is pending.
        /// </summary>
        public int NextState => m_nextState;

        /// <summary>
        /// Gets the current active state ID.
        /// </summary>
        public int CurrentState => m_currentState;

        /// <summary>
        /// Gets the name of the previous state.
        /// </summary>
        public string PreviousStateName => GetStateName(m_previousState);

        /// <summary>
        /// Gets the name of the next queued state.
        /// </summary>
        public string NextStateName => GetStateName(m_nextState);

        /// <summary>
        /// Gets the name of the current state.
        /// </summary>
        public string CurrentStateName => GetStateName(m_currentState);

        /// <summary>
        /// Returns true if a state change is queued.
        /// </summary>
        public bool IsChangingStates => IsStateValid(m_nextState);

        /// <summary>
        /// Gets the time (in seconds) spent in the current state.
        /// </summary>
        public float TimeInCurrentState => m_timeInCurrentState;

        /// <summary>
        /// Gets the total number of state transitions that have occurred.
        /// </summary>
        public int TransitionCount => m_transitionCount;

        /// <summary>
        /// Gets whether this FSM has been disposed.
        /// </summary>
        public bool IsDisposed => m_disposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new FSM with default history size of 10.
        /// </summary>
        public Fsm() : this(10)
        {
        }

        /// <summary>
        /// Creates a new FSM with specified history size.
        /// </summary>
        /// <param name="historySize">Maximum number of previous states to track</param>
        public Fsm(int historySize)
        {
            m_currentState = NO_STATE;
            m_nextState = NO_STATE;
            m_previousState = NO_STATE;
            m_disposed = false;
            m_timeInCurrentState = 0f;
            m_transitionCount = 0;
            m_maxHistorySize = Mathf.Max(1, historySize);

            m_stateMap = new Dictionary<int, State>();
            m_stateNames = new Dictionary<int, string>();
            m_stateHistory = new Queue<int>(m_maxHistorySize);

            // Legacy support
            m_states = new List<State>();
            m_nextAutoIndex = 0;
        }

        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Fsm()
        {
            Dispose(false);
        }

        #endregion

        #region State Management

        /// <summary>
        /// Adds a state using auto-incrementing index (legacy method, maintained for backward compatibility).
        /// </summary>
        /// <param name="definition">The state function</param>
        /// <remarks>
        /// This method assigns states sequential integer IDs starting at 0.
        /// For better control, use AddState(int, State) instead.
        /// </remarks>
        public void AddState(State definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            m_states.Add(definition);
            int stateId = m_nextAutoIndex++;
            m_stateMap[stateId] = definition;
            m_stateNames[stateId] = definition.Method.Name;
        }

        /// <summary>
        /// Adds a state with an explicit state ID.
        /// </summary>
        /// <param name="stateId">The state identifier (typically cast from enum)</param>
        /// <param name="definition">The state function</param>
        /// <example>
        /// <code>
        /// fsm.AddState((int)EntityState.Idle, IdleState);
        /// fsm.AddState((int)CharacterState.Jump, JumpState);
        /// </code>
        /// </example>
        public void AddState(int stateId, State definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (m_stateMap.ContainsKey(stateId))
            {
                Debug.LogWarning($"[Fsm] State {stateId} already exists. Replacing.");
            }

            m_stateMap[stateId] = definition;
            m_stateNames[stateId] = definition.Method.Name;
        }

        /// <summary>
        /// Adds a state with an explicit state ID and custom name.
        /// </summary>
        /// <param name="stateId">The state identifier</param>
        /// <param name="definition">The state function</param>
        /// <param name="customName">Custom name for this state (for debugging)</param>
        public void AddState(int stateId, State definition, string customName)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));

            if (string.IsNullOrEmpty(customName))
                customName = definition.Method.Name;

            m_stateMap[stateId] = definition;
            m_stateNames[stateId] = customName;
        }

        /// <summary>
        /// Removes a state from the FSM.
        /// </summary>
        /// <param name="stateId">The state to remove</param>
        /// <returns>True if the state was removed</returns>
        public bool RemoveState(int stateId)
        {
            if (stateId == m_currentState)
            {
                Debug.LogWarning($"[Fsm] Cannot remove current state {stateId}");
                return false;
            }

            bool removed = m_stateMap.Remove(stateId);
            m_stateNames.Remove(stateId);
            return removed;
        }

        /// <summary>
        /// Checks if a state ID is valid and registered.
        /// </summary>
        /// <param name="state">The state ID to check</param>
        /// <returns>True if the state is valid</returns>
        public bool IsStateValid(int state)
        {
            if (m_disposed)
                return false;

            return state != NO_STATE && m_stateMap.ContainsKey(state);
        }

        /// <summary>
        /// Checks if the current state is valid.
        /// </summary>
        /// <returns>True if current state is valid</returns>
        public bool IsCurrentStateValid()
        {
            return IsStateValid(m_currentState);
        }

        /// <summary>
        /// Gets the name of a state.
        /// </summary>
        /// <param name="stateId">The state ID</param>
        /// <returns>The state name, or INVALID_STATE_NAME if not found</returns>
        public string GetStateName(int stateId)
        {
            if (m_stateNames.TryGetValue(stateId, out string name))
                return name;

            // Fallback to legacy list-based lookup
            if (stateId >= 0 && stateId < m_states.Count && m_states[stateId] != null)
                return m_states[stateId].Method.Name;

            return INVALID_STATE_NAME;
        }

        /// <summary>
        /// Gets all registered state IDs.
        /// </summary>
        /// <returns>Array of state IDs</returns>
        public int[] GetAllStates()
        {
            var states = new int[m_stateMap.Count];
            m_stateMap.Keys.CopyTo(states, 0);
            return states;
        }

        #endregion

        #region State Transitions

        /// <summary>
        /// Queues a state change to occur on the next Tick.
        /// </summary>
        /// <param name="state">The target state ID</param>
        /// <remarks>
        /// State changes are queued and applied during Tick() to ensure consistent timing.
        /// Use ForceState() for immediate state changes.
        /// </remarks>
        public void ChangeState(int state)
        {
            if (m_disposed)
            {
                Debug.LogWarning("[Fsm] Cannot change state on disposed FSM");
                return;
            }

            m_nextState = state;
        }

        /// <summary>
        /// Attempts to queue a state change without throwing exceptions.
        /// </summary>
        /// <param name="state">The target state ID</param>
        /// <returns>True if the state is valid and was queued</returns>
        public bool TryChangeState(int state)
        {
            if (m_disposed || !IsStateValid(state))
                return false;

            m_nextState = state;
            return true;
        }

        /// <summary>
        /// Immediately forces a state change without waiting for next Tick.
        /// </summary>
        /// <param name="state">The target state ID</param>
        /// <param name="deltaTime">Delta time to pass to Enter/Exit callbacks (default: 0)</param>
        /// <remarks>
        /// Use this when you need an immediate state change that can't wait for the next Tick.
        /// This will execute Exit and Enter callbacks immediately.
        /// </remarks>
        public void ForceState(int state, float deltaTime = 0f)
        {
            if (m_disposed)
            {
                Debug.LogWarning("[Fsm] Cannot force state on disposed FSM");
                return;
            }

            if (!IsStateValid(state))
            {
                Debug.LogError($"[Fsm] Cannot force invalid state: {state}");
                return;
            }

            ChangeStateInternal(state, deltaTime);
        }

        /// <summary>
        /// Attempts to immediately force a state change without throwing exceptions.
        /// </summary>
        /// <param name="state">The target state ID</param>
        /// <param name="deltaTime">Delta time to pass to callbacks</param>
        /// <returns>True if the state change succeeded</returns>
        public bool TryForceState(int state, float deltaTime = 0f)
        {
            if (m_disposed || !IsStateValid(state))
                return false;

            ChangeStateInternal(state, deltaTime);
            return true;
        }

        /// <summary>
        /// Cancels a pending state change.
        /// </summary>
        public void CancelPendingChange()
        {
            m_nextState = NO_STATE;
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Updates the state machine, processing queued state changes and calling Update on the current state.
        /// </summary>
        /// <param name="deltaTime">Time since last update</param>
        public void Tick(float deltaTime)
        {
            if (m_disposed)
                return;

            // Process queued state change
            if (IsChangingStates)
            {
                ChangeStateInternal(m_nextState, deltaTime);
                m_nextState = NO_STATE;
            }

            // Update current state
            if (IsStateValid(m_currentState))
            {
                m_stateMap[m_currentState]?.Invoke(StateStep.Update, deltaTime);
                m_timeInCurrentState += deltaTime;
            }
        }

        /// <summary>
        /// Updates the state machine for physics, calling FixedUpdate on the current state.
        /// </summary>
        /// <param name="fixedDeltaTime">Fixed delta time</param>
        public void PhysicsTick(float fixedDeltaTime)
        {
            if (m_disposed)
                return;

            if (IsStateValid(m_currentState))
            {
                m_stateMap[m_currentState]?.Invoke(StateStep.FixedUpdate, fixedDeltaTime);
            }
        }

        #endregion

        #region State History

        /// <summary>
        /// Gets the state history as an array (oldest to newest).
        /// </summary>
        /// <returns>Array of previous state IDs</returns>
        public int[] GetStateHistory()
        {
            return m_stateHistory.ToArray();
        }

        /// <summary>
        /// Gets the last N states from history.
        /// </summary>
        /// <param name="count">Number of states to retrieve</param>
        /// <returns>Array of state IDs (up to count)</returns>
        public int[] GetStateHistory(int count)
        {
            int[] history = m_stateHistory.ToArray();
            int takeCount = Mathf.Min(count, history.Length);
            int[] result = new int[takeCount];
            Array.Copy(history, history.Length - takeCount, result, 0, takeCount);
            return result;
        }

        /// <summary>
        /// Clears the state history.
        /// </summary>
        public void ClearHistory()
        {
            m_stateHistory.Clear();
        }

        #endregion

        #region Disposal

        /// <summary>
        /// Disposes the FSM and clears all states.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Protected disposal implementation.
        /// </summary>
        /// <param name="disposing">True if called from Dispose(), false if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (m_disposed)
                return;

            if (disposing)
            {
                // Exit current state
                if (IsStateValid(m_currentState))
                {
                    try
                    {
                        m_stateMap[m_currentState]?.Invoke(StateStep.Exit, 0f);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[Fsm] Error during state exit on dispose: {ex.Message}");
                    }
                }

                // Clear events
                OnStateChanged = null;
                OnStateEntered = null;
                OnStateExited = null;

                // Clear data structures
                m_states.Clear();
                m_stateMap.Clear();
                m_stateNames.Clear();
                m_stateHistory.Clear();
            }

            m_disposed = true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Internal method that performs the actual state change.
        /// </summary>
        private void ChangeStateInternal(int newState, float deltaTime)
        {
            int oldState = m_currentState;

            // Exit current state
            if (IsStateValid(m_currentState))
            {
                try
                {
                    OnStateExited?.Invoke(m_currentState);
                    m_stateMap[m_currentState]?.Invoke(StateStep.Exit, deltaTime);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Fsm] Error during state exit: {ex.Message}");
                }
            }

            // Update state tracking
            m_previousState = m_currentState;
            m_currentState = newState;
            m_timeInCurrentState = 0f;
            m_transitionCount++;

            // Add to history
            if (newState != NO_STATE)
            {
                m_stateHistory.Enqueue(newState);
                while (m_stateHistory.Count > m_maxHistorySize)
                {
                    m_stateHistory.Dequeue();
                }
            }

            // Invoke state changed event
            try
            {
                OnStateChanged?.Invoke(oldState, newState);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Fsm] Error in OnStateChanged event: {ex.Message}");
            }

            // Enter new state
            if (IsStateValid(m_currentState))
            {
                try
                {
                    m_stateMap[m_currentState]?.Invoke(StateStep.Enter, deltaTime);
                    OnStateEntered?.Invoke(m_currentState);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Fsm] Error during state enter: {ex.Message}");
                }
            }
        }

        #endregion
    }
}
