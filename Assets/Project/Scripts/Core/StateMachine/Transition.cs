using System;

namespace ActionCombat.Core.StateMachine
{
    /// <summary>
    /// Defines a conditional link between two states.
    /// The state machine evaluates these each frame to determine transitions.
    /// </summary>
    public class Transition
    {
        public State TargetState { get; private set; }
        public Func<bool> Condition { get; private set; }
        public string Name { get; private set; }

        /// <param name="name">Debug-friendly name (e.g., "Idle→Run")</param>
        /// <param name="targetState">State to transition to</param>
        /// <param name="condition">Returns true when transition should fire</param>
        public Transition(string name, State targetState, Func<bool> condition)
        {
            Name = name;
            TargetState = targetState;
            Condition = condition;
        }
    }
}