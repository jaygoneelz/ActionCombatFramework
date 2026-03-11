using System;
using System.Collections.Generic;
using UnityEngine;

namespace ActionCombat.Core.StateMachine
{
    /// <summary>
    /// Generic finite state machine. Manages state lifecycle, transitions,
    /// and exposes debug data.
    /// </summary>
    public class StateMachine
    {
        public State CurrentState { get; private set; }
        public State PreviousState { get; private set; }
        public float TimeInCurrentState => CurrentState?.TimeInState ?? 0f;

        // Debug support
        public event Action<State, State> OnStateChanged;
        private List<StateTransitionRecord> transitionHistory = new List<StateTransitionRecord>();
        private const int MAX_HISTORY = 20;

        public IReadOnlyList<StateTransitionRecord> TransitionHistory => transitionHistory;

        /// <summary>
        /// Sets the initial state. Call once during setup.
        /// </summary>
        public void Initialise(State startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
            RecordTransition(null, startingState, "Initialise");
        }

        /// <summary>
        /// Call from MonoBehaviour.Update(). Executes current state
        /// then evaluates transitions.
        /// </summary>
        public void Update()
        {
            if (CurrentState == null) return;

            CurrentState.Execute();

            Transition triggered = CurrentState.EvaluateTransitions();
            if (triggered != null)
            {
                ChangeState(triggered.TargetState, triggered.Name);
            }
        }

        /// <summary>
        /// Call from MonoBehaviour.FixedUpdate() for physics-based state logic.
        /// </summary>
        public void PhysicsUpdate()
        {
            CurrentState?.PhysicsExecute();
        }

        /// <summary>
        /// Force an immediate state change. Use for interrupts like
        /// taking damage, dying, or stagger.
        /// </summary>
        public void ForceState(State newState, string reason = "Forced")
        {
            ChangeState(newState, reason);
        }

        private void ChangeState(State newState, string reason)
        {
            if (newState == CurrentState) return;

            PreviousState = CurrentState;
            CurrentState.Exit();

            State oldState = CurrentState;
            CurrentState = newState;
            CurrentState.Enter();

            RecordTransition(oldState, newState, reason);
            OnStateChanged?.Invoke(oldState, newState);
        }

        private void RecordTransition(State from, State to, string reason)
        {
            transitionHistory.Add(new StateTransitionRecord
            {
                FromState = from?.Name ?? "None",
                ToState = to?.Name ?? "None",
                Reason = reason,
                Time = Time.time
            });

            if (transitionHistory.Count > MAX_HISTORY)
            {
                transitionHistory.RemoveAt(0);
            }
        }
    }

    /// <summary>
    /// Immutable record of a state transition for debug display.
    /// </summary>
    public struct StateTransitionRecord
    {
        public string FromState;
        public string ToState;
        public string Reason;
        public float Time;

        public override string ToString()
        {
            return $"[{Time:F2}] {FromState} → {ToState} ({Reason})";
        }
    }
}