using ActionCombat.Core.StateMachine;
using ActionCombat.Combat;
using UnityEngine;

namespace ActionCombat.Player.States
{
    public class HeavyAttackState : PlayerState
    {
        private float stepTimer;
        private float attackDuration = 0.7f;
        private bool hitboxActivated;

        private HitboxController hitbox;

        public bool IsComplete { get; private set; }

        public HeavyAttackState(StateMachine stateMachine, PlayerController player)
            : base("HeavyAttack", stateMachine, player)
        {
            hitbox = player.GetComponentInChildren<HitboxController>();
        }

        public override void Enter()
        {
            base.Enter();
            stepTimer = 0f;
            IsComplete = false;
            hitboxActivated = false;
            animator.PlayAnimation("HeavyAttack", 0.1f);

            UnityEngine.Debug.Log("[Combat] Heavy Attack | Damage: 25");
        }

        public override void Execute()
        {
            base.Execute();

            stepTimer += Time.deltaTime;
            float normalisedTime = stepTimer / attackDuration;

            // Hitbox active: 30% to 55% (slower windup than light)
            if (normalisedTime >= 0.3f && normalisedTime < 0.55f)
            {
                if (!hitboxActivated)
                {
                    hitboxActivated = true;
                    if (hitbox != null) hitbox.Activate();
                }
            }
            else if (normalisedTime >= 0.55f && hitboxActivated)
            {
                hitboxActivated = false;
                if (hitbox != null) hitbox.Deactivate();
            }

            // Forward lunge
            if (normalisedTime > 0.2f && normalisedTime < 0.5f)
            {
                Vector3 forward = player.transform.forward * 4f * Time.deltaTime;
                movement.ApplyRootMotion(forward);
            }

            if (normalisedTime >= 1f)
            {
                if (hitbox != null) hitbox.Deactivate();
                IsComplete = true;
            }
        }

        public override void Exit()
        {
            base.Exit();
            if (hitbox != null) hitbox.Deactivate();
            hitboxActivated = false;
        }
    }
}