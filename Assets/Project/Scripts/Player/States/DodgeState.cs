using ActionCombat.Core.StateMachine;
using UnityEngine;

namespace ActionCombat.Player.States
{
    public class DodgeState : PlayerState
    {
        private float dodgeTimer;
        private float dodgeDuration = 0.5f;
        private float dodgeSpeed = 12f;
        private Vector3 dodgeDirection;

        public bool IsComplete { get; private set; }

        /// <summary>
        /// Invincibility frames: true during the first 70% of the dodge.
        /// Combat system checks this to ignore incoming damage.
        /// </summary>
        public bool HasIFrames => dodgeTimer / dodgeDuration < 0.7f;

        public DodgeState(StateMachine stateMachine, PlayerController player)
            : base("Dodge", stateMachine, player) { }

        public override void Enter()
        {
            base.Enter();
            dodgeTimer = 0f;
            IsComplete = false;

            // Dodge in input direction, or backwards if no input
            Vector2 moveInput = input.MoveInput;
            if (moveInput.sqrMagnitude > 0.1f)
            {
                Transform cam = Camera.main.transform;
                Vector3 camForward = cam.forward;
                Vector3 camRight = cam.right;
                camForward.y = 0f;
                camRight.y = 0f;
                camForward.Normalize();
                camRight.Normalize();
                dodgeDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;
            }
            else
            {
                dodgeDirection = -player.transform.forward;
            }

            animator.PlayAnimation("Dodge", 0.05f);

            UnityEngine.Debug.Log("[Combat] Dodge");
        }

        public override void Execute()
        {
            base.Execute();

            dodgeTimer += Time.deltaTime;
            float normalisedTime = dodgeTimer / dodgeDuration;

            // Ease-out movement (fast start, slow end)
            float speedMultiplier = 1f - (normalisedTime * normalisedTime);
            Vector3 displacement = dodgeDirection * dodgeSpeed * speedMultiplier * Time.deltaTime;
            movement.ApplyRootMotion(displacement);

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