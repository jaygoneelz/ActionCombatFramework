using ActionCombat.Core.StateMachine;
using UnityEngine;

namespace ActionCombat.AI
{
    public class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(StateMachine stateMachine, EnemyController enemy)
            : base("EnemyChase", stateMachine, enemy) { }

        public override void Enter()
        {
            base.Enter();
            UnityEngine.Debug.Log($"[AI] {enemy.gameObject.name} chasing player");
        }

        public override void Execute()
        {
            base.Execute();
            enemy.RotateTowardsTarget();
            enemy.MoveTowardsTarget();
        }
    }
}