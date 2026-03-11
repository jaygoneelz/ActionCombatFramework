namespace ActionCombat.Core.StateMachine
{
    /// <summary>
    /// Contract for all states in the state machine.
    /// Implement this for custom state behaviour.
    /// </summary>
    public interface IState
    {
        void Enter();
        void Execute();
        void PhysicsExecute();
        void Exit();
    }
}