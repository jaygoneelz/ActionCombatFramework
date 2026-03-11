using System.Collections.Generic;

namespace ActionCombat.Core.StateMachine
{
    /// <summary>
    /// Base class for all states. Provides transition management
    /// and common state functionality. Extend this for every state.
    /// </summary>
    public abstract class State : IState
    {
        protected StateMachine stateMachine;
        private List<Transition> transitions = new List<Transition>();

        public string Name { get; private set; }
        public float TimeInState { get; private set; }
        public IReadOnlyList<Transition> Transitions => transitions;

        public State(string name, StateMachine stateMachine)
        {
            Name = name;
            this.stateMachine = stateMachine;
        }

        public void AddTransition(Transition transition)
        {
            transitions.Add(transition);
        }

        /// <summary>
        /// Evaluates all transitions in priority order.
        /// Returns the first transition whose condition is met.
        /// </summary>
        public Transition EvaluateTransitions()
        {
            for (int i = 0; i < transitions.Count; i++)
            {
                if (transitions[i].Condition())
                {
                    return transitions[i];
                }
            }
            return null;
        }

        public virtual void Enter()
        {
            TimeInState = 0f;
        }

        public virtual void Execute()
        {
            TimeInState += UnityEngine.Time.deltaTime;
        }

        public virtual void PhysicsExecute() { }

        public virtual void Exit() { }
    }
}