using ActionCombat.Core.StateMachine;
using UnityEngine;

namespace ActionCombat.AI
{
    public class EnemyIdleState : EnemyState
    {
        public EnemyIdleState(StateMachine stateMachine, EnemyController enemy)
            : base("EnemyIdle", stateMachine, enemy) { }

        public override void Enter()
        {
            base.Enter();
        }

        public override void Execute()
        {
            base.Execute();
            // Stand still, wait for player to enter detection range
        }
    }
}