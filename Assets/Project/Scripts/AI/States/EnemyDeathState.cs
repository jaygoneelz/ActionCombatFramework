using ActionCombat.Core.StateMachine;
using UnityEngine;

namespace ActionCombat.AI
{
    public class EnemyDeathState : EnemyState
    {
        public EnemyDeathState(StateMachine stateMachine, EnemyController enemy)
            : base("EnemyDeath", stateMachine, enemy) { }

        public override void Enter()
        {
            base.Enter();

            // Deactivate hitbox
            if (enemy.Hitbox != null) enemy.Hitbox.Deactivate();

            UnityEngine.Debug.Log($"[AI] {enemy.gameObject.name} DIED");

            // Disable after delay so death is visible
            GameObject.Destroy(enemy.gameObject, 2f);
        }

        public override void Execute()
        {
            // Dead — do nothing
        }
    }
}