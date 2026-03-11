using ActionCombat.Core.StateMachine;
using UnityEngine;

namespace ActionCombat.AI
{
    public class EnemyStaggerState : EnemyState
    {
        private float staggerTimer;
        private float staggerDuration;

        public bool IsComplete { get; private set; }

        public EnemyStaggerState(StateMachine stateMachine, EnemyController enemy)
            : base("EnemyStagger", stateMachine, enemy) { }

        public void SetStaggerDuration(float duration)
        {
            staggerDuration = duration;
        }

        public override void Enter()
        {
            base.Enter();
            staggerTimer = 0f;
            IsComplete = false;

            // Deactivate hitbox if enemy was mid-attack
            if (enemy.Hitbox != null) enemy.Hitbox.Deactivate();

            UnityEngine.Debug.Log($"[AI] {enemy.gameObject.name} staggered for {staggerDuration:F2}s");
        }

        public override void Execute()
        {
            base.Execute();

            staggerTimer += Time.deltaTime;

            if (staggerTimer >= staggerDuration)
            {
                IsComplete = true;
            }
        }
    }
}