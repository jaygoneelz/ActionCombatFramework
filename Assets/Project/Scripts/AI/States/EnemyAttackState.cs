using ActionCombat.Core.StateMachine;
using UnityEngine;

namespace ActionCombat.AI
{
    public class EnemyAttackState : EnemyState
    {
        private float attackTimer;
        private float attackDuration = 0.6f;
        private bool hitboxActivated;

        public bool IsComplete { get; private set; }

        public EnemyAttackState(StateMachine stateMachine, EnemyController enemy)
            : base("EnemyAttack", stateMachine, enemy) { }

        public override void Enter()
        {
            base.Enter();
            attackTimer = 0f;
            IsComplete = false;
            hitboxActivated = false;
            enemy.LastAttackTime = Time.time;

            // Face the player before attacking
            enemy.RotateTowardsTarget();

            UnityEngine.Debug.Log($"[AI] {enemy.gameObject.name} attacking!");
        }

        public override void Execute()
        {
            base.Execute();

            attackTimer += Time.deltaTime;
            float normalisedTime = attackTimer / attackDuration;

            // Hitbox active: 25% to 50%
            if (normalisedTime >= 0.25f && normalisedTime < 0.5f)
            {
                if (!hitboxActivated && enemy.Hitbox != null)
                {
                    hitboxActivated = true;
                    enemy.Hitbox.Activate();
                }
            }
            else if (normalisedTime >= 0.5f && hitboxActivated)
            {
                hitboxActivated = false;
                if (enemy.Hitbox != null) enemy.Hitbox.Deactivate();
            }

            // Small lunge forward
            if (normalisedTime > 0.15f && normalisedTime < 0.4f)
            {
                Vector3 forward = enemy.transform.forward * 3f * Time.deltaTime;
                forward.y = -9.81f * Time.deltaTime;
                if (enemy.CharController != null)
                    enemy.CharController.Move(forward);
            }

            if (normalisedTime >= 1f)
            {
                if (enemy.Hitbox != null) enemy.Hitbox.Deactivate();
                IsComplete = true;
            }
        }

        public override void Exit()
        {
            base.Exit();
            if (enemy.Hitbox != null) enemy.Hitbox.Deactivate();
            hitboxActivated = false;
        }
    }
}