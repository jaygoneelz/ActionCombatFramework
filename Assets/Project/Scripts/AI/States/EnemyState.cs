using ActionCombat.Core.StateMachine;

namespace ActionCombat.AI
{
    public abstract class EnemyState : State
    {
        protected EnemyController enemy;

        public EnemyState(string name, StateMachine stateMachine, EnemyController enemy)
            : base(name, stateMachine)
        {
            this.enemy = enemy;
        }
    }
}