using ActionCombat.Core.StateMachine;
using UnityEngine;

namespace ActionCombat.Player.States
{
    public class HeavyAttackState : PlayerState
    {
        private float stepTimer;
        private float attackDuration = 0.7f;

        public bool IsComplete { get; private set; }

        public HeavyAttackState(StateMachine stateMachine, PlayerController player)
            : base("HeavyAttack", stateMachine, player) { }

        public override void Enter()
        {
            base.Enter();
            stepTimer = 0f;
            IsComplete = false;
            animator.PlayAnimation("HeavyAttack", 0.1f);

            UnityEngine.Debug.Log("[Combat] Heavy Attack");
        }

        public override void Execute()
        {
            base.Execute();

            stepTimer += Time.deltaTime;
            float normalisedTime = stepTimer / attackDuration;

            // Stronger forward lunge for heavy attack
            if (normalisedTime > 0.2f && normalisedTime < 0.5f)
            {
                Vector3 forward = player.transform.forward * 4f * Time.deltaTime;
                movement.ApplyRootMotion(forward);
            }

            if (normalisedTime >= 1f)
            {
                IsComplete = true;
            }
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}